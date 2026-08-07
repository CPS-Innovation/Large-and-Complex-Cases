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
import { registerCase } from "../helpers/case-api";

/**
 * Seeds a complete fixture from scratch: a fresh Egress workspace with test
 * files, a newly registered case, and the Egress connection between them.
 *
 * Unlike register-case.setup.ts this is headless - no browser, no NetApp
 * connect - so it can seed an environment whose UI login is unavailable, and
 * it leaves no storageState behind for other projects to consume.
 *
 * File size/count come from TEST_FILE_SIZE_MB / TEST_FILE_COUNT (default 10MB x1).
 *
 *   ENVIRONMENT=dev npx playwright test --project=seed-fixture
 */
setup("seed a workspace, case and Egress connection", async () => {
  setup.setTimeout(900_000);

  const config = loadEnvConfig();
  const caseApiBaseUrl = process.env.CASE_API_BASE_URL;
  const lccApiBaseUrl = process.env.LCC_API_BASE_URL;
  const cmrcClientId = process.env.CMRC_API_CLIENT_ID;

  if (!caseApiBaseUrl) throw new Error("CASE_API_BASE_URL must be set");
  if (!lccApiBaseUrl) throw new Error("LCC_API_BASE_URL must be set");
  if (!cmrcClientId) throw new Error("CMRC_API_CLIENT_ID must be set");

  const fileSizeMb = config.testFileSizeMb;
  const fileCount = config.testFileCount;

  console.log("=== Seeding fixture ===");

  console.log("[1/5] Authenticating with Egress...");
  const egressToken = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );

  console.log("[2/5] Creating workspace...");
  const workspaceName = await findNextWorkspaceName(
    config.egressBaseUrl,
    egressToken
  );
  const workspaceId = await createWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceName,
    config.egressTemplateId
  );
  await addUserToWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceId,
    config.e2eAdUser,
    config.egressAdminRoleId
  );
  console.log(`      ${workspaceName} (${workspaceId})`);

  console.log(`[3/5] Uploading ${fileCount} x ${fileSizeMb}MB...`);
  const uploaded: { fileName: string; fileSize: number }[] = [];
  for (let i = 1; i <= fileCount; i++) {
    const stamp = new Date().toISOString().replace(/[:.]/g, "-").slice(0, 19);
    const fileName = `generated-${fileSizeMb}MB-${stamp}-file${i}.txt`;
    const uploadId = await uploadFile(
      config.egressBaseUrl,
      egressToken,
      config.egressServiceAccountAuth,
      workspaceId,
      fileSizeMb * 1024 * 1024,
      fileName
    );
    const file = await getUploadedFile(
      config.egressBaseUrl,
      egressToken,
      config.egressServiceAccountAuth,
      workspaceId,
      uploadId,
      {
        timeoutMs: Math.max(30_000, fileSizeMb * 15_000),
        retryDelay: Math.min(10_000, Math.max(1_000, fileSizeMb * 5)),
      }
    );
    uploaded.push({ fileName: file.fileName, fileSize: file.fileSize });
    console.log(`      ${file.fileName} (${file.fileSize} bytes)`);
  }

  console.log("[4/5] Registering case...");
  // The case API and the LCC API have different audiences, so mint one token
  // per API rather than reusing a single bearer.
  const caseAuth = await getAuthTokens(
    config.tenantId,
    cmrcClientId,
    config.e2eAdUser,
    config.e2eAdPassword,
    config.ddeiBaseUrl,
    config.ddeiAccessKeyCaseRegister,
    config.cmsUsername,
    config.cmsPassword
  );
  const { caseId, caseUrn } = await registerCase(
    caseApiBaseUrl,
    caseAuth.accessToken,
    caseAuth.cmsAuth,
    workspaceName
  );
  console.log(`      ${caseUrn} (${caseId})`);

  console.log("[5/5] Connecting Egress workspace to the case...");
  const lccAuth = await getAuthTokens(
    config.tenantId,
    config.lccApiClientId,
    config.e2eAdUser,
    config.e2eAdPassword,
    config.ddeiBaseUrl,
    config.ddeiAccessKeyCaseRegister,
    config.cmsUsername,
    config.cmsPassword
  );
  const connect = await fetch(`${lccApiBaseUrl}/api/v1/egress/connections`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${lccAuth.accessToken}`,
      Cookie: `Cms-Auth-Values=${lccAuth.cmsAuth}`,
      "Correlation-Id": crypto.randomUUID(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      egressWorkspaceId: workspaceId,
      egressWorkspaceName: workspaceName,
      caseId,
    }),
  });
  if (!connect.ok) {
    throw new Error(
      `Egress connection failed (${connect.status}): ${(await connect.text()).slice(0, 300)}`
    );
  }

  console.log("\n=== Seeded ===");
  console.log(`CASE_URN=${caseUrn}`);
  console.log(`CASE_ID=${caseId}`);
  console.log(`WORKSPACE_ID=${workspaceId}`);
  console.log(`WORKSPACE_NAME=${workspaceName}`);
  uploaded.forEach((f) => console.log(`FILE=${f.fileName} (${f.fileSize} bytes)`));
});
