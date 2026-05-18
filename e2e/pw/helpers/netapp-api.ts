/**
 * Best-effort NetApp file delete via the LCC backend's batch endpoint.
 *
 * Endpoint: POST /api/v1/netapp/delete/batch
 * Body:    { "caseId": <int>, "operations": [{ "type": "Material",
 *            "sourcePath": "<netAppFolderPath>/<fileName>" }, ...] }
 *
 * The backend validates each `sourcePath` starts with the case's stored
 * NetappFolderPath (DeleteNetAppBatch.cs:81), so the caller must supply
 * the same folder prefix the case is connected to — default mode reads
 * NETAPP_OPERATION_NAME from env, register-case mode uses
 * REGISTER_CASE_NETAPP_FOLDER (helpers/constants.ts).
 *
 * Filenames are basenames here. Tests select individual files via
 * `selectEgressFileByName(...)`, which makes the SPA send
 * `sourcePaths: [{ path: "" }]` (empty relativePath); the backend
 * writes at `<destinationPath>/<basename>`, i.e. flat at the NetApp
 * folder root. Tests that switch to folder/multi-file selection would
 * preserve the source path and need to pass the full NetApp-relative
 * path here.
 *
 * Auth: requires both an Azure AD bearer token AND a CMS-Auth cookie
 * value (URL-encoded JSON), the same pair returned by
 * helpers/auth-api.ts `getAuthTokens`.
 *
 * This call 403s in production by design, so any 4xx is logged and
 * swallowed. Same warn-don't-throw pattern as the Egress teardown
 * helpers. No-op when caseId, baseUrl or netAppFolderPath are missing
 * (env not configured, or default-mode caseId never plumbed through).
 */
export async function deleteNetAppFiles(
  baseUrl: string,
  caseId: number | undefined,
  netAppFolderPath: string,
  fileNames: string[],
  accessToken: string,
  cmsAuth: string
): Promise<void> {
  if (!baseUrl || !caseId || !netAppFolderPath || fileNames.length === 0) {
    if (fileNames.length > 0) {
      console.warn(
        `  [teardown] deleteNetAppFiles skipped — missing LCC_API_BASE_URL, caseId or NetApp folder`
      );
    }
    return;
  }
  const folderPrefix = netAppFolderPath.endsWith("/")
    ? netAppFolderPath
    : `${netAppFolderPath}/`;
  const operations = fileNames.map((name) => ({
    type: "Material",
    sourcePath: `${folderPrefix}${name}`,
  }));
  try {
    const response = await fetch(`${baseUrl}/api/v1/netapp/delete/batch`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        Cookie: `Cms-Auth-Values=${cmsAuth}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ caseId, operations }),
    });
    if (!response.ok) {
      const text = await response.text();
      console.warn(
        `  [teardown] deleteNetAppFiles (${fileNames.length} file(s)) failed (${response.status}): ${text.slice(0, 200)}`
      );
      return;
    }
    const body = (await response.json()) as {
      status: string;
      succeeded: number;
      failed: number;
      notFound: number;
    };
    if (body.failed > 0) {
      console.warn(
        `  [teardown] deleteNetAppFiles: ${body.failed} of ${fileNames.length} failed (status=${body.status})`
      );
    }
  } catch (err) {
    console.warn(`  [teardown] deleteNetAppFiles threw:`, err);
  }
}

/**
 * Best-effort disassociation of a case's NetApp connection.
 *
 * Endpoint: DELETE /api/v1/netapp/connections?case-id={caseId}
 * Auth:     Bearer AAD token only.
 *
 * Used by register-case teardown after a successful run so the freshly
 * registered case stops pointing at the NetApp folder. Best-effort —
 * a non-OK response is logged and swallowed; the run-end workspace
 * sweep still proceeds.
 */
export async function disassociateNetAppConnection(
  baseUrl: string,
  caseId: number,
  accessToken: string
): Promise<void> {
  if (!baseUrl) {
    console.warn(
      `  [teardown] disassociateNetAppConnection skipped — missing LCC_API_BASE_URL`
    );
    return;
  }
  try {
    const response = await fetch(
      `${baseUrl}/api/v1/netapp/connections?case-id=${encodeURIComponent(String(caseId))}`,
      {
        method: "DELETE",
        headers: { Authorization: `Bearer ${accessToken}` },
      }
    );
    if (!response.ok) {
      const text = await response.text();
      console.warn(
        `  [teardown] disassociateNetAppConnection caseId=${caseId} failed (${response.status}): ${text.slice(0, 200)}`
      );
    }
  } catch (err) {
    console.warn(
      `  [teardown] disassociateNetAppConnection caseId=${caseId} threw:`,
      err
    );
  }
}
