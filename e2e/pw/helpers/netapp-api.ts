/**
 * Best-effort NetApp file delete via the LCC backend.
 *
 * Endpoint: DELETE /api/v1/cases/{operationName}/netapp
 * Body:    { "path": "<file basename>" }
 * The backend prefixes the path with `${operationName}/`, so passing
 * operationName="Automation-Testing" and path="generated-...txt" deletes
 * the S3 object at "Automation-Testing/generated-...txt".
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
