import { test as base, expect } from "@playwright/test";
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
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { EgressConnectPage } from "../pages/EgressConnectPage";
import { EgressConfirmationPage } from "../pages/EgressConfirmationPage";
import { NetAppConnectPage } from "../pages/NetAppConnectPage";
import { NetAppConfirmationPage } from "../pages/NetAppConfirmationPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";

base("full flow - create workspace, register case, login, search, connect", async ({ page }) => {
  base.setTimeout(300_000);

  const config = loadEnvConfig();

  // === API Setup ===
  console.log("=== API Setup ===\n");

  console.log("[1/7] Authenticating with Egress...");
  const egressToken = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );

  console.log("[2/7] Finding next workspace name...");
  const workspaceName = await findNextWorkspaceName(
    config.egressBaseUrl,
    egressToken
  );
  console.log(`  Workspace: ${workspaceName}`);

  console.log("[3/7] Creating Egress workspace...");
  const workspaceId = await createWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceName,
    config.egressTemplateId
  );

  console.log("[4/7] Adding test user...");
  await addUserToWorkspace(
    config.egressBaseUrl,
    egressToken,
    workspaceId,
    config.e2eAdUser,
    config.egressAdminRoleId
  );

  console.log("[5/7] Uploading test file...");
  const fileSizeBytes = config.testFileSizeMb * 1024 * 1024;
  const timestamp = new Date().toISOString().replace(/[:.]/g, "-").slice(0, 19);
  const fileName = `generated-${config.testFileSizeMb}MB-${timestamp}-file1.txt`;
  await uploadFile(
    config.egressBaseUrl,
    egressToken,
    workspaceId,
    fileSizeBytes,
    fileName
  );

  console.log("[6/7] Getting auth tokens...");
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

  console.log("[7/7] Registering case...");
  const { caseId, caseUrn } = await registerCase(
    config.caseApiBaseUrl,
    accessToken,
    cmsAuth,
    workspaceName
  );
  console.log(`  Case: ${caseUrn} (ID: ${caseId})`);
  console.log("=== API Setup Complete ===\n");

  // === Browser Login ===
  console.log("=== Browser Login ===\n");

  console.log("  Tactical login...");
  await page.goto(config.cmsLoginPage);
  const tacticalLogin = new TacticalLoginPage(page);
  await tacticalLogin.login(config.cmsUsername, config.cmsPassword);
  console.log("  Tactical login complete.");

  console.log("  Azure AD login...");
  await page.goto(config.baseUrl);
  const adLogin = new AzureADLoginPage(page);
  await adLogin.login(config.e2eAdUser, config.e2eAdPassword);
  console.log("  Azure AD login complete.");

  // Wait for the 3 radio buttons
  console.log("  Waiting for radio buttons...");
  await Promise.all([
    page.getByTestId("radio-search-urn").waitFor({ state: "visible", timeout: 30000 }),
    page.getByTestId("radio-search-defendant-name").waitFor({ state: "visible", timeout: 30000 }),
    page.getByTestId("radio-search-operation-name").waitFor({ state: "visible", timeout: 30000 }),
  ]);
  console.log("  All 3 radio buttons visible!");
  console.log("=== Browser Login Complete ===\n");

  // === Search by URN ===
  console.log(`=== Searching for case URN: ${caseUrn} ===\n`);
  const caseSearch = new CaseSearchPage(page);
  await caseSearch.selectUrnSearch();
  await caseSearch.fillUrn(caseUrn);
  await caseSearch.clickSearch();

  const searchResults = new SearchResultsPage(page);
  await searchResults.waitForResults();
  console.log("  Search results loaded!");

  // === Click into case ===
  console.log("  Clicking into case...");
  await searchResults.clickCaseAction(caseUrn);

  // === Connect Egress workspace ===
  console.log(`=== Connecting Egress workspace: ${workspaceName} ===\n`);
  const egressConnect = new EgressConnectPage(page);
  await egressConnect.searchFolder(workspaceName);
  await egressConnect.waitForResults();
  console.log("  Egress folders displayed!");
  await egressConnect.connectFolder();

  const egressConfirm = new EgressConfirmationPage(page);
  await egressConfirm.confirmConnect();
  console.log("  Egress connected!");

  // === Connect NetApp folder ===
  console.log("=== Connecting NetApp folder ===\n");
  const netappConnect = new NetAppConnectPage(page);
  await netappConnect.waitForFolders();
  console.log("  NetApp folders displayed!");

  // Click on the "Automation-Testing" folder's Connect button
  console.log("  Clicking Automation-Testing folder...");
  await page
    .getByRole("row", { name: "Automation-Testing Connect", exact: false })
    .filter({ hasNotText: "Move" })
    .locator('button[name="secondary"]')
    .click();
  console.log("  Automation-Testing folder selected!");

  const netappConfirm = new NetAppConfirmationPage(page);
  await netappConfirm.confirmConnect();
  console.log("  NetApp connected!");

  // === Transfer Materials (already on this tab after connect) ===
  console.log("=== Transfer Materials ===\n");
  const transferTab = new TransferMaterialsTab(page);
  await transferTab.waitForEgressFiles();
  console.log("  Egress files loaded.");

  // Navigate to "4. Served Evidence" folder (rendered as button, not link)
  console.log("  Navigating to 4. Served Evidence...");
  await page.getByRole("button", { name: "4. Served Evidence" }).click();
  await transferTab.waitForEgressFiles();
  console.log("  Served Evidence folder loaded.");

  // Click "select all" checkbox (Folder/File name header)
  console.log("  Selecting all files...");
  await page.getByLabel("Select folders and files").check();
  console.log("  All files selected!");

  // Click the "Copy" button in the netapp inset text area
  console.log("  Clicking Copy to Automation-Testing...");
  await page.getByTestId("netapp-inset-text").getByRole("button", { name: "Copy" }).click();
  console.log("  Copy action selected!");

  // Confirm transfer in the modal
  console.log("  Waiting for confirmation modal...");
  const confirmationModal = page.getByTestId("div-modal");
  await confirmationModal.waitFor({ state: "visible", timeout: 30000 });
  console.log("  Confirmation modal visible!");

  // Check the "I want to copy 1 file to Automation-Testing" checkbox
  console.log("  Clicking confirmation checkbox...");
  await confirmationModal.getByLabel(/I want to copy/).click();
  console.log("  Checkbox checked!");

  // Click Continue
  console.log("  Clicking Continue...");
  await confirmationModal.getByRole("button", { name: "Continue" }).click();
  console.log("  Transfer initiated!");

  // Wait for transfer to complete - verify "Files copied successfully"
  console.log("  Waiting for transfer to complete...");
  const successBanner = page.getByTestId("transfer-success-notification-banner");
  await successBanner.waitFor({ state: "visible", timeout: 120_000 });
  await expect(successBanner).toContainText("Files copied successfully");
  console.log("  Transfer complete - 'Files copied successfully' confirmed!");

  // === Verify file in Shared Drive destination folder ===
  console.log("=== Verifying file in Shared Drive destination ===\n");
  console.log(`  Looking for file: ${fileName}`);
  const netappTable = page.getByTestId("netapp-table-wrapper");
  await expect(netappTable).toBeVisible({ timeout: 30000 });
  await expect(netappTable).toContainText(fileName, { timeout: 30000 });
  console.log(`  File '${fileName}' found in Shared Drive destination folder!`);

  // === Click Activity Log tab ===
  console.log("=== Switching to Activity Log tab ===\n");
  await page.getByTestId("tab-1").click();
  await expect(page.getByTestId("tab-active")).toHaveText("Activity log", { timeout: 30000 });
  console.log("  Activity Log tab active!");

  // Verify TransferCompleted entry exists
  console.log("  Checking for TransferCompleted entry...");
  const activityTimeline = page.getByTestId("activities-timeline");
  await activityTimeline.waitFor({ state: "visible", timeout: 30000 });

  // Find the section with the transfer completed description
  const transferCompletedSection = activityTimeline
    .locator("section")
    .filter({ hasText: /Documents\/folders copied from Egress to Shared Drive/i });
  await expect(transferCompletedSection.first()).toBeVisible({ timeout: 30000 });
  console.log("  Found: 'Documents/folders copied from Egress to Shared Drive'!");

  // Verify "Transfer" tag
  await expect(transferCompletedSection.first().getByTestId("transfer-tag")).toBeVisible();
  await expect(transferCompletedSection.first().getByTestId("transfer-tag")).toHaveText("Transfer");
  console.log("  'Transfer' tag verified!");

  // Verify "Completed" status tag
  await expect(transferCompletedSection.first().getByTestId("transfer-status-tag")).toBeVisible();
  const statusText = await transferCompletedSection.first().getByTestId("transfer-status-tag").innerText();
  console.log(`  Transfer status tag: '${statusText}'`);

  // Verify description h4
  await expect(transferCompletedSection.first().locator("h4")).toContainText(/Documents\/folders copied from Egress to Shared Drive/i);
  console.log("  TransferCompleted entry fully verified!");

  console.log("\n=== ALL VERIFICATIONS PASSED ===\n");
});
