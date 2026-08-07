import { test as setup } from "@playwright/test";
import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  findNextWorkspaceName,
  createWorkspace,
  addUserToWorkspace,
  uploadFile,
  getUploadedFile,
} from "../helpers/egress-api";
import { getAuthTokens } from "../helpers/auth-api";

/**
 * Rebuilds the default-mode Egress fixture after the workspace named by
 * DEFAULT_WORKSPACE_ID has gone missing (Egress returns 404 "The Case was
 * not found on the server" and every *-default.spec.ts fails in setup).
 *
 * Creates a fresh AUTOMATION-TESTING* workspace, adds the E2E user, uploads
 * the long-name path-length fixture, and connects the workspace to the
 * existing DEFAULT_CASE_ID. Prints the new id/name to paste into
 * .env.staging as DEFAULT_WORKSPACE_ID / DEFAULT_WORKSPACE_NAME.
 *
 * Opt-in only:
 *   ENVIRONMENT=staging npx playwright test --project=rebuild-default-fixture
 */

// 200 characters, matching the fixture used for the path-length ACs.
const LONG_FILE_NAME =
  "PATHTEST260_CP_0672_TALBOT_billing_summary_reconciliation_working_papers_" +
  "supporting_evidence_attachments_all_suspects_operation_montrose_final_" +
  "reviewed_approved_disclosure_bundle_extended_edition.xlsx";

/** True when the case already holds the given NetApp folder. */
async function isNetAppFolderOwnedBy(
  lccApiBaseUrl: string,
  headers: Record<string, string>,
  caseId: string,
  operationName: string
): Promise<boolean> {
  const response = await fetch(`${lccApiBaseUrl}/api/v1/cases/${caseId}`, {
    headers,
  });
  if (!response.ok) return false;
  const metadata = await response.json();
  const held: string = metadata.netappFolderPath ?? "";
  return held.replace(/\/$/, "") === operationName.replace(/\/$/, "");
}

setup("rebuild the default-mode Egress fixture", async () => {
  setup.setTimeout(600_000);

  const config = loadEnvConfig();
  const caseId = process.env.DEFAULT_CASE_ID;
  const caseUrn = process.env.DEFAULT_CASE_URN;
  const lccApiBaseUrl = process.env.LCC_API_BASE_URL;

  if (!caseId || !caseUrn) {
    throw new Error("DEFAULT_CASE_ID / DEFAULT_CASE_URN must be set");
  }
  if (!lccApiBaseUrl) {
    throw new Error("LCC_API_BASE_URL must be set to connect the workspace");
  }
  if (LONG_FILE_NAME.length !== 200) {
    throw new Error(`long file name is ${LONG_FILE_NAME.length} chars, expected 200`);
  }

  console.log("=== Rebuilding default-mode fixture ===");
  console.log(`  case ${caseUrn} (${caseId})`);

  console.log("[1/6] Authenticating with Egress...");
  const egressToken = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );

  console.log("[2/6] Finding next workspace name...");
  const workspaceName = await findNextWorkspaceName(
    config.egressBaseUrl,
    egressToken
  );
  console.log(`  ${workspaceName}`);

  console.log("[3/6] Creating workspace...");
  const workspaceId = await createWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceName,
    config.egressTemplateId
  );
  console.log(`  ${workspaceId}`);

  console.log("[4/6] Adding E2E user...");
  await addUserToWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceId,
    config.e2eAdUser,
    config.egressAdminRoleId
  );

  console.log(`[5/6] Uploading path-length fixture (${LONG_FILE_NAME.length} chars)...`);
  const uploadId = await uploadFile(
    config.egressBaseUrl,
    egressToken,
    config.egressServiceAccountAuth,
    workspaceId,
    1024 * 1024,
    LONG_FILE_NAME,
    { folderPath: "4. Served Evidence/" }
  );
  const uploaded = await getUploadedFile(
    config.egressBaseUrl,
    egressToken,
    config.egressServiceAccountAuth,
    workspaceId,
    uploadId,
    { timeoutMs: 120_000, retryDelay: 2_000 }
  );
  console.log(`  uploaded: ${uploaded.fileName} (${uploaded.fileSize} bytes)`);

  console.log("[6/6] Connecting workspace to the existing case...");
  const { accessToken, cmsAuth } = await getAuthTokens(
    config.tenantId,
    config.lccApiClientId,
    config.e2eAdUser,
    config.e2eAdPassword,
    config.ddeiBaseUrl,
    config.ddeiAccessKeyCaseRegister,
    config.cmsUsername,
    config.cmsPassword
  );

  const headers = {
    Authorization: `Bearer ${accessToken}`,
    Cookie: `Cms-Auth-Values=${cmsAuth}`,
    "Correlation-Id": crypto.randomUUID(),
    "Content-Type": "application/json",
  };
  const connectionsUrl = `${lccApiBaseUrl}/api/v1/egress/connections`;

  // A connection pointing at the deleted workspace may still be recorded
  // against this case; the API rejects a second one, so clear it first.
  const disconnect = await fetch(connectionsUrl, {
    method: "DELETE",
    headers,
    body: JSON.stringify({ caseId: Number(caseId) }),
  });
  console.log(`  existing connection removed: HTTP ${disconnect.status}`);

  const connect = await fetch(connectionsUrl, {
    method: "POST",
    headers,
    body: JSON.stringify({
      egressWorkspaceId: workspaceId,
      egressWorkspaceName: workspaceName,
      caseId: Number(caseId),
    }),
  });
  const connectBody = await connect.text();
  if (!connect.ok) {
    throw new Error(
      `Egress connection failed (${connect.status}): ${connectBody.slice(0, 300)}`
    );
  }
  console.log(`  Egress connected: HTTP ${connect.status}`);

  // The NetApp side matters just as much. Without it the app diverts to
  // "Link a Shared Drive folder to the case" and every default-mode spec
  // times out waiting for a transfer control that is never rendered.
  const netappOperationName = process.env.NETAPP_OPERATION_NAME;
  if (!netappOperationName) {
    throw new Error("NETAPP_OPERATION_NAME must be set to connect the Shared Drive");
  }
  const netappConnect = await fetch(`${lccApiBaseUrl}/api/v1/netapp/connections`, {
    method: "POST",
    headers,
    body: JSON.stringify({
      caseId: Number(caseId),
      operationName: netappOperationName,
      folderPath: `${netappOperationName}/`,
    }),
  });
  const netappBody = await netappConnect.text();
  if (!netappConnect.ok) {
    // A 409 means the folder is already claimed. That is only a problem when
    // some OTHER case holds it — when this case already holds it the fixture
    // is in the state we wanted, and only the Egress half needed rebuilding.
    const alreadyOurs =
      netappConnect.status === 409 &&
      (await isNetAppFolderOwnedBy(
        lccApiBaseUrl,
        headers,
        caseId,
        netappOperationName
      ));
    if (!alreadyOurs) {
      throw new Error(
        `NetApp connection failed (${netappConnect.status}): ${netappBody.slice(0, 300)}\n` +
          `The folder is held by a different case. Free it with ` +
          `DELETE ${lccApiBaseUrl}/api/v1/netapp/connections?case-id=<holder>.`
      );
    }
    console.log(
      `  NetApp already connected to this case (${netappOperationName}) - left as is`
    );
  } else {
    console.log(
      `  NetApp connected: HTTP ${netappConnect.status} (${netappOperationName})`
    );
  }

  console.log("\n=== Fixture rebuilt - update .env.staging ===");
  console.log(`DEFAULT_WORKSPACE_ID=${workspaceId}`);
  console.log(`DEFAULT_WORKSPACE_NAME=${workspaceName}`);
  console.log(`LONG_FILE_NAME=${LONG_FILE_NAME}`);
  console.log(`LONG_FILE_NAME_LENGTH=${LONG_FILE_NAME.length}`);
});
