import { test, expect } from "../fixtures/test-fixtures";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { EgressConnectPage } from "../pages/EgressConnectPage";
import { EgressConfirmationPage } from "../pages/EgressConfirmationPage";
import { NetAppConnectPage } from "../pages/NetAppConnectPage";
import { NetAppConfirmationPage } from "../pages/NetAppConfirmationPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test("full flow - create workspace, register case, login, search, connect", async ({
  page,
  testData,
}) => {
  test.setTimeout(300_000);
  const { caseUrn, workspace, files } = testData;

  // Step 1: Search by URN
  console.log(`=== Searching for case URN: ${caseUrn} ===\n`);
  const caseSearch = new CaseSearchPage(page);
  await caseSearch.selectUrnSearch();
  await caseSearch.fillUrn(caseUrn);
  await caseSearch.clickSearch();

  const searchResults = new SearchResultsPage(page);
  await searchResults.waitForResults();
  console.log("  Search results loaded!");

  // Step 2: Click into case
  console.log("  Clicking into case...");
  await searchResults.clickCaseAction(caseUrn);

  // Step 3: Connect Egress workspace
  console.log(`=== Connecting Egress workspace: ${workspace.name} ===\n`);
  const egressConnect = new EgressConnectPage(page);
  await egressConnect.searchFolder(workspace.name);
  await egressConnect.waitForResults();
  console.log("  Egress folders displayed!");
  await egressConnect.connectFolder();

  const egressConfirm = new EgressConfirmationPage(page);
  await egressConfirm.confirmConnect();
  console.log("  Egress connected!");

  // Step 4: Connect NetApp folder
  console.log("=== Connecting NetApp folder ===\n");
  const netappConnect = new NetAppConnectPage(page);
  await netappConnect.waitForFolders();
  console.log("  NetApp folders displayed!");

  console.log("  Clicking Automation-Testing folder...");
  await netappConnect.connectFolder();
  console.log("  Automation-Testing folder selected!");

  const netappConfirm = new NetAppConfirmationPage(page);
  await netappConfirm.confirmConnect();
  console.log("  NetApp connected!");

  // Step 5: Transfer Materials
  console.log("=== Transfer Materials ===\n");
  const caseMgmt = new CaseManagementPage(page);
  await caseMgmt.waitForLoad();

  const transferTab = new TransferMaterialsTab(page);
  await transferTab.waitForEgressFiles();
  console.log("  Egress files loaded.");

  console.log("  Navigating to 4. Served Evidence...");
  await transferTab.navigateToFolder("4. Served Evidence");
  await transferTab.waitForEgressFiles();
  console.log("  Served Evidence folder loaded.");

  console.log("  Selecting all files...");
  await transferTab.selectAllEgressFiles();
  console.log("  All files selected!");

  console.log("  Clicking Copy to Automation-Testing...");
  await transferTab.selectAction("Copy");
  console.log("  Copy action selected!");

  // Step 6: Confirm transfer
  console.log("  Waiting for confirmation modal...");
  await transferTab.confirmTransfer();
  console.log("  Transfer initiated!");

  // Step 7: Wait for transfer to complete
  console.log("  Waiting for transfer to complete...");
  await transferTab.waitForTransferComplete();
  console.log("  Transfer complete - 'Files copied successfully' confirmed!");

  // Step 8: Verify file in Shared Drive destination
  console.log("=== Verifying file in Shared Drive destination ===\n");
  const fileName = files[0].fileName;
  console.log(`  Looking for file: ${fileName}`);
  const netappTable = page.getByTestId("netapp-table-wrapper");
  await expect(netappTable).toBeVisible({ timeout: 30000 });
  await expect(netappTable).toContainText(fileName, { timeout: 30000 });
  console.log(`  File '${fileName}' found in Shared Drive destination folder!`);

  // Step 9: Verify Activity Log
  console.log("=== Switching to Activity Log tab ===\n");
  await caseMgmt.switchToTab("activity-log");

  const activityLog = new ActivityLogTab(page);
  await activityLog.waitForLogs();
  console.log("  Activity Log tab active!");

  console.log("  Checking for TransferCompleted entry...");
  await activityLog.verifyTransferLogged("Copy");
  console.log("  TransferCompleted entry fully verified!");

  console.log("\n=== ALL VERIFICATIONS PASSED ===\n");
});
