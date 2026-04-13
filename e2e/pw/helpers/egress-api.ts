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

export async function findNextWorkspaceName(
  baseUrl: string,
  token: string
): Promise<string> {
  let maxNumber = 0;
  let page = 1;
  const pageSize = 50;

  while (true) {
    const response = await fetch(
      `${baseUrl}/api/v1/workspaces/?page=${page}`,
      { headers: { Authorization: `Basic ${token}` } }
    );

    if (!response.ok) break;

    const data = await response.json();
    const workspaces: { name: string }[] = data.data || data.results || data;

    if (!Array.isArray(workspaces) || workspaces.length === 0) break;

    for (const ws of workspaces) {
      const match = ws.name.match(/^AUTOMATION-TESTING(\d+)(-\d+)?$/i);
      if (match) {
        const num = parseInt(match[1], 10);
        if (num > maxNumber) maxNumber = num;
      }
    }

    if (workspaces.length < pageSize) break;
    page++;
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

  // The file ID will be discovered later via the LCC API during tests.
  // The Egress file listing API doesn't reliably expose subfolder contents,
  // so we use the upload ID as reference and trust the upload completed.
  console.log(`  Upload complete: ${uploadId}`);
  return { fileId: uploadId, fileName, fileSize: fileSizeBytes };
}

