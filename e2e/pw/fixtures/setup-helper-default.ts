import { Page } from "@playwright/test";
import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  uploadFile,
} from "../helpers/egress-api";
import { TacticalLoginPage } from "../pages/TacticalLoginPage";
import { AzureADLoginPage } from "../pages/AzureADLoginPage";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import type { TestSetupResult, UploadedFile } from "../helpers/types";

export interface DefaultSetupOptions {
  fileSizeMb?: number;
  fileCount?: number;
}

/**
 * Default mode setup: uploads files to a known workspace and uses an existing
 * case that already has Egress and NetApp connections.
 * Skips workspace creation, case registration, and user addition.
 */
export async function setupDefaultTestData(
  page: Page,
  options: DefaultSetupOptions = {}
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

  const workspaceId = config.defaultWorkspaceId;
  const workspaceName = config.defaultWorkspaceName;
  const caseUrn = config.defaultCaseUrn;
  const caseId = config.defaultCaseId;

  if (!workspaceId) {
    throw new Error(
      "DEFAULT_WORKSPACE_ID is required for default mode. Set it in your .env file."
    );
  }

  // Step 1: Authenticate with Egress and upload files to existing workspace
  console.log("=== Default Mode: Upload to Existing Workspace ===\n");

  console.log("[1/2] Authenticating with Egress...");
  const egressToken = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );

  console.log(
    `[2/2] Uploading ${fileCount} test file(s) of ${fileSizeMb}MB to workspace ${workspaceName} (${workspaceId})...`
  );
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

  console.log("=== Upload Complete ===\n");

  console.log(
    `  Using existing case: ${caseUrn} (ID: ${caseId}), workspace: ${workspaceName}`
  );
  console.log("  Skipping case registration (already connected)\n");

  // Step 2: Browser login
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
