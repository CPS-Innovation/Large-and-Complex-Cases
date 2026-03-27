import { Page } from "@playwright/test";
import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  findNextWorkspaceName,
  createWorkspace,
  addUserToWorkspace,
  uploadFile,
} from "../helpers/egress-api";
import { getAuthTokens } from "../helpers/auth-api";
import { registerCase } from "../helpers/case-api";
import { TacticalLoginPage } from "../pages/TacticalLoginPage";
import { AzureADLoginPage } from "../pages/AzureADLoginPage";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import type { TestSetupResult, UploadedFile } from "../helpers/types";

export interface SetupOptions {
  fileSizeMb?: number;
  fileCount?: number;
}

export async function setupTestData(
  page: Page,
  options: SetupOptions = {}
): Promise<TestSetupResult> {
  const config = loadEnvConfig();
  const fileSizeMb = options.fileSizeMb ?? config.testFileSizeMb;
  const fileCount = options.fileCount ?? config.testFileCount;

  const {
    CMS_USERNAME,
    CMS_PASSWORD,
    E2E_AD_USER,
    E2E_AD_PASSWORD,
    CMS_LOGIN_PAGE,
    BASE_URL,
  } = process.env;

  // Step 1: Create Egress workspace and upload files
  console.log("=== Workspace Setup ===\n");

  console.log("[1/5] Authenticating with Egress...");
  const egressToken = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );

  console.log("[2/5] Finding next workspace name...");
  const workspaceName = await findNextWorkspaceName(
    config.egressBaseUrl,
    egressToken
  );
  console.log(`  Workspace: ${workspaceName}`);

  console.log("[3/5] Creating Egress workspace...");
  const workspaceId = await createWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceName,
    config.egressTemplateId
  );

  console.log("[4/5] Adding test user...");
  await addUserToWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceId,
    config.e2eAdUser,
    config.egressAdminRoleId
  );

  console.log(`[5/5] Uploading ${fileCount} test file(s) of ${fileSizeMb}MB each...`);
  const fileSizeBytes = fileSizeMb * 1024 * 1024;
  const files: UploadedFile[] = [];

  for (let i = 1; i <= fileCount; i++) {
    const timestamp = new Date()
      .toISOString()
      .replace(/[:.]/g, "-")
      .slice(0, 19);
    const fileName = `generated-${fileSizeMb}MB-${timestamp}-file${i}.txt`;
    console.log(`  Uploading ${fileName} (${i}/${fileCount})...`);
    const file = await uploadFile(
      config.egressBaseUrl,
      egressToken,
      workspaceId,
      fileSizeBytes,
      fileName
    );
    files.push(file);
  }

  console.log("=== Workspace Setup Complete ===\n");

  // Step 2: Get auth tokens and register a fresh case
  console.log("=== Case Registration ===\n");

  console.log("  Getting auth tokens...");
  const { accessToken, cmsAuth } = await getAuthTokens(
    config.tenantId,
    config.clientId,
    config.e2eAdUser,
    config.e2eAdPassword,
    config.ddeiBaseUrl,
    config.ddeiAccessKey,
    config.cmsUsername,
    config.cmsPassword
  );

  console.log("  Registering fresh case...");
  const { caseId, caseUrn } = await registerCase(
    config.caseApiBaseUrl,
    accessToken,
    cmsAuth,
    workspaceName
  );
  console.log(`  Case: ${caseUrn} (ID: ${caseId})`);

  console.log("=== Case Registration Complete ===\n");

  // Step 3: Browser login
  console.log("=== Browser Login ===\n");

  console.log("  Tactical login...");
  await page.goto(CMS_LOGIN_PAGE!);
  const tacticalLogin = new TacticalLoginPage(page);
  await tacticalLogin.login(CMS_USERNAME!, CMS_PASSWORD!);
  console.log("  Tactical login complete.");

  console.log("  Azure AD login...");
  await page.goto(BASE_URL!);
  const adLogin = new AzureADLoginPage(page);
  await adLogin.login(E2E_AD_USER!, E2E_AD_PASSWORD!);
  console.log("  Azure AD login complete.");

  console.log("  Waiting for radio buttons...");
  const caseSearch = new CaseSearchPage(page);
  await caseSearch.waitForRadioButtons();
  console.log("  All 3 radio buttons visible!");

  console.log("=== Browser Login Complete ===\n");

  return {
    workspace: { id: workspaceId, name: workspaceName },
    files,
    caseId,
    caseUrn,
  };
}
