/**
 * Best-effort NetApp file delete via the LCC backend.
 *
 * Endpoint: DELETE /api/v1/cases/{operationName}/netapp
 * Body:    { "path": "<file basename>" }
 * The backend prefixes the path with `${operationName}/`, so passing
 * operationName="Automation-Testing" and path="generated-...txt" deletes
 * the S3 object at "Automation-Testing/generated-...txt".
 *
 * **Basename only is correct here**, not a defensive guess. Tests select
 * individual files via `selectEgressFileByName(...)`, which makes the SPA
 * send `sourcePaths: [{ path: "" }]` (empty relativePath); the backend
 * writes at `<destinationPath>/<basename>`, i.e. flat at the NetApp
 * folder root. A failed-transfer trace with
 * `sourceRootFolderPath: "4. Served Evidence//"` and `destinationPath: "/"`
 * confirms this. Tests that switch to folder/multi-file selection would
 * preserve the source path and need the full NetApp-relative path passed
 * here — but that's a per-test concern, not a backend contract drift.
 *
 * Auth: requires both an Azure AD bearer token AND a CMS-Auth cookie value
 * (URL-encoded JSON), the same pair returned by helpers/auth-api.ts
 * `getAuthTokens`.
 *
 * This call 403s in production by design (see the backend Run() method),
 * so any 4xx is logged and swallowed. Same warn-don't-throw pattern as the
 * Egress teardown helpers.
 */
export async function deleteNetAppFile(
  baseUrl: string,
  operationName: string,
  fileName: string,
  accessToken: string,
  cmsAuth: string
): Promise<void> {
  if (!baseUrl || !operationName) {
    console.warn(
      `  [teardown] deleteNetAppFile skipped — missing LCC_API_BASE_URL or NETAPP_OPERATION_NAME`
    );
    return;
  }
  try {
    const response = await fetch(
      `${baseUrl}/api/v1/cases/${encodeURIComponent(operationName)}/netapp`,
      {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${accessToken}`,
          "Cms-Auth-Values": cmsAuth,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ path: fileName }),
      }
    );
    if (!response.ok) {
      const text = await response.text();
      console.warn(
        `  [teardown] deleteNetAppFile ${fileName} failed (${response.status}): ${text.slice(0, 200)}`
      );
    }
  } catch (err) {
    console.warn(`  [teardown] deleteNetAppFile ${fileName} threw:`, err);
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
