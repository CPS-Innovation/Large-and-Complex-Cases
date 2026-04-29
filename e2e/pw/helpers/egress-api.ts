import type { UploadedFile } from "./types";

export async function authenticateEgress(
  baseUrl: string,
  serviceAccountAuth: string
): Promise<string> {
  const response = await fetch(`${baseUrl}/api/v1/user/auth/`, {
    method: "GET",
    headers: { Authorization: `Basic ${serviceAccountAuth}` },
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Egress auth failed (${response.status}): ${text}`);
  }

  const data = await response.json();
  // Egress API expects the token to be Base64-encoded and sent as Basic auth
  return Buffer.from(data.token, "utf-8").toString("base64");
}

export interface EgressWorkspaceInfo {
  id: string;
  name: string;
  /** ISO datetime from Egress `date_created`. */
  dateCreated: string;
}

/**
 * Paginates /workspaces?view=full and returns every AUTOMATION-TESTING*
 * workspace with its creation timestamp. Used by the register-case teardown
 * to sweep workspaces older than N hours regardless of which run created
 * them. Workspaces without a date_created field are skipped (safer to leave
 * them than to delete blindly).
 */
export async function listAutomationWorkspaces(
  baseUrl: string,
  token: string
): Promise<EgressWorkspaceInfo[]> {
  const pageSize = 50;
  const maxPages = 50;
  const results: EgressWorkspaceInfo[] = [];

  for (let page = 1; page <= maxPages; page++) {
    const response = await fetch(
      `${baseUrl}/api/v1/workspaces/?page=${page}&page_size=${pageSize}&view=full`,
      { headers: { Authorization: `Basic ${token}` } }
    );
    if (!response.ok) {
      const text = await response.text();
      throw new Error(
        `Egress workspace list failed (page ${page}, status ${response.status}): ${text}`
      );
    }

    const body: {
      data?: { id: string; name: string; date_created?: string }[];
    } = await response.json();
    const workspaces = body.data ?? [];
    if (workspaces.length === 0) break;

    for (const ws of workspaces) {
      if (/^AUTOMATION-TESTING/i.test(ws.name) && ws.date_created) {
        results.push({
          id: ws.id,
          name: ws.name,
          dateCreated: ws.date_created,
        });
      }
    }

    if (workspaces.length < pageSize) break;
  }

  return results;
}

export async function findNextWorkspaceName(
  baseUrl: string,
  token: string
): Promise<string> {
  const pageSize = 50;
  const maxPages = 50;
  let maxNumber = 0;

  for (let page = 1; page <= maxPages; page++) {
    const response = await fetch(
      `${baseUrl}/api/v1/workspaces/?page=${page}&page_size=${pageSize}`,
      { headers: { Authorization: `Basic ${token}` } }
    );

    if (!response.ok) {
      const text = await response.text();
      throw new Error(
        `Egress workspace list failed (page ${page}, status ${response.status}): ${text}`
      );
    }

    // Documented shape: { data: ListWorkspacesResponseData[], data_info: {...} }
    // See backend/CPS.ComplexCases.Egress/Models/Response/FindWorkspaceResponse.cs
    const body: { data?: { name: string }[] } = await response.json();
    const workspaces = body.data ?? [];

    if (workspaces.length === 0) break;

    for (const ws of workspaces) {
      const match = ws.name.match(/^AUTOMATION-TESTING(\d+)(-\d+)?$/i);
      if (match) {
        const num = parseInt(match[1], 10);
        if (num > maxNumber) maxNumber = num;
      }
    }

    if (workspaces.length < pageSize) break;
  }

  // Add random suffix to avoid race conditions when running tests in parallel
  const randomSuffix = Math.floor(Math.random() * 900 + 100);
  return `AUTOMATION-TESTING${maxNumber + 1}-${randomSuffix}`;
}

export async function createWorkspace(
  baseUrl: string,
  token: string,
  name: string,
  templateId: string
): Promise<string> {
  const response = await fetch(`${baseUrl}/api/v1/workspaces/`, {
    method: "POST",
    headers: {
      Authorization: `Basic ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      name,
      template_id: templateId,
      description: `E2E test workspace created at ${new Date().toISOString()}`,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `Egress workspace creation failed (${response.status}): ${text}`
    );
  }

  const data = await response.json();
  return data.id;
}

/**
 * Creates a subfolder under the given parent path in the workspace.
 * Idempotent: if Egress reports the folder already exists (4xx), this is
 * treated as success so the helper is safe to call from every test run
 * when uploading into a dated subfolder.
 */
export async function createFolder(
  baseUrl: string,
  token: string,
  workspaceId: string,
  parentPath: string,
  folderName: string
): Promise<void> {
  const response = await fetch(
    `${baseUrl}/api/v1/workspaces/${workspaceId}/files?path=${encodeURIComponent(parentPath)}`,
    {
      method: "POST",
      headers: {
        Authorization: `Basic ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ folder_name: folderName }),
    }
  );

  if (response.ok) return;

  const text = await response.text();
  // Treat duplicate-folder responses as success.
  if (
    response.status === 409 ||
    (response.status === 400 && /exist/i.test(text))
  ) {
    return;
  }
  throw new Error(
    `Egress folder creation failed (${response.status}): ${text}`
  );
}

export async function addUserToWorkspace(
  baseUrl: string,
  token: string,
  workspaceId: string,
  email: string,
  roleId: string
): Promise<void> {
  const response = await fetch(
    `${baseUrl}/api/v1/workspaces/${workspaceId}/users/`,
    {
      method: "POST",
      headers: {
        Authorization: `Basic ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify([{ switch_id: email, role_id: roleId }]),
    }
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `Failed to add user to workspace (${response.status}): ${text}`
    );
  }
}

export async function uploadFile(
  baseUrl: string,
  token: string,
  workspaceId: string,
  fileSizeBytes: number,
  fileName: string,
  folderPath: string = "4. Served Evidence/",
  chunkSizeMB: number = 5
): Promise<UploadedFile> {
  // Step 1: Initiate upload
  const initiateResponse = await fetch(
    `${baseUrl}/api/v1/workspaces/${workspaceId}/uploads`,
    {
      method: "POST",
      headers: {
        Authorization: `Basic ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        filename: fileName,
        filesize: fileSizeBytes,
        folder_path: folderPath,
      }),
    }
  );

  if (!initiateResponse.ok) {
    const text = await initiateResponse.text();
    throw new Error(`Upload initiation failed (${initiateResponse.status}): ${text}`);
  }

  const uploadData = await initiateResponse.json();
  const uploadId = uploadData.id;

  // Step 2: Upload chunks
  const chunkSize = chunkSizeMB * 1024 * 1024;
  let offset = 0;

  while (offset < fileSizeBytes) {
    const end = Math.min(offset + chunkSize, fileSizeBytes);
    const currentChunkSize = end - offset;
    const chunkData = Buffer.alloc(currentChunkSize, 0x41); // fill with 'A'

    const formData = new FormData();
    formData.append(
      "file_content",
      new Blob([chunkData]),
      fileName
    );

    let uploaded = false;
    for (let attempt = 0; attempt < 3; attempt++) {
      const chunkResponse = await fetch(
        `${baseUrl}/api/v1/workspaces/${workspaceId}/uploads/${uploadId}/`,
        {
          method: "PATCH",
          headers: {
            Authorization: `Basic ${token}`,
            "Content-Range": `bytes ${offset}-${end - 1}/${fileSizeBytes}`,
          },
          body: formData,
        }
      );

      if (chunkResponse.ok) {
        uploaded = true;
        break;
      }

      if (attempt === 2) {
        const text = await chunkResponse.text();
        throw new Error(
          `Chunk upload failed after 3 attempts (${chunkResponse.status}): ${text}`
        );
      }

      await new Promise((r) => setTimeout(r, 2000));
    }

    if (!uploaded) {
      throw new Error("Chunk upload failed unexpectedly");
    }

    offset = end;
    console.log(
      `  Uploaded ${Math.round((offset / fileSizeBytes) * 100)}% of ${fileName}`
    );
  }

  // Step 3: Complete upload
  const completeResponse = await fetch(
    `${baseUrl}/api/v1/workspaces/${workspaceId}/uploads/${uploadId}/`,
    {
      method: "PUT",
      headers: {
        Authorization: `Basic ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ done: true }),
    }
  );

  if (!completeResponse.ok) {
    const text = await completeResponse.text();
    throw new Error(
      `Upload completion failed (${completeResponse.status}): ${text}`
    );
  }

  // Egress returns the file record on completion. Fall back to uploadId if
  // the response shape changes so callers that need an id for teardown
  // always get something to work with.
  const completeData = await completeResponse.json().catch(() => ({}));
  const fileId: string = completeData?.id ?? uploadId;

  console.log(`  Upload complete: ${fileId}`);
  return { id: fileId, fileName, fileSize: fileSizeBytes };
}

/**
 * Best-effort bulk file delete. Logs and swallows errors so teardown never
 * fails a passing test — the dated subfolder + manual sweep acts as a
 * safety net. Endpoint shape matches EgressRequestFactory.DeleteFilesRequest
 * in the backend: DELETE /workspaces/{id}/files with { file_ids: [...] }.
 */
export async function deleteFiles(
  baseUrl: string,
  token: string,
  workspaceId: string,
  fileIds: string[]
): Promise<void> {
  if (fileIds.length === 0) return;
  try {
    const response = await fetch(
      `${baseUrl}/api/v1/workspaces/${workspaceId}/files`,
      {
        method: "DELETE",
        headers: {
          Authorization: `Basic ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ file_ids: fileIds }),
      }
    );
    if (!response.ok) {
      const text = await response.text();
      console.warn(
        `  [teardown] deleteFiles [${fileIds.join(",")}] failed (${response.status}): ${text}`
      );
    }
  } catch (err) {
    console.warn(`  [teardown] deleteFiles threw:`, err);
  }
}

/**
 * Best-effort workspace delete. Intended for register-case teardown; must
 * NOT be called against DEFAULT_WORKSPACE_ID. Endpoint per Egress docs:
 * DELETE /api/v1/workspaces/{workspace_id}/ — trailing slash is required,
 * Egress 405s without it.
 */
export async function deleteWorkspace(
  baseUrl: string,
  token: string,
  workspaceId: string
): Promise<void> {
  try {
    const response = await fetch(
      `${baseUrl}/api/v1/workspaces/${workspaceId}/`,
      {
        method: "DELETE",
        headers: { Authorization: `Basic ${token}` },
      }
    );
    if (!response.ok) {
      const text = await response.text();
      console.warn(
        `  [teardown] deleteWorkspace ${workspaceId} failed (${response.status}): ${text}`
      );
    }
  } catch (err) {
    console.warn(
      `  [teardown] deleteWorkspace ${workspaceId} threw:`,
      err
    );
  }
}

