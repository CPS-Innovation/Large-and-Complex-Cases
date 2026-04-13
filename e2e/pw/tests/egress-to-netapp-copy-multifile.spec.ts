import { test, expect } from "../fixtures/test-fixtures-multifile";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { EgressConnectPage } from "../pages/EgressConnectPage";
import { EgressConfirmationPage } from "../pages/EgressConfirmationPage";
import { NetAppConnectPage } from "../pages/NetAppConnectPage";
import { NetAppConfirmationPage } from "../pages/NetAppConfirmationPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("Egress to NetApp Copy - Multiple Files (50MB x 3)", () => {
  test("should copy 3 files from Egress to NetApp", async ({
    page,
    testData,
  }) => {
    test.setTimeout(600_000);
    const { caseUrn, workspace } = testData;

    // Step 1: Search for case by URN
    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    // Step 2: Click into case from results
    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    // Step 3: Connect Egress workspace
    const egressConnect = new EgressConnectPage(page);
    await egressConnect.searchFolder(workspace.name);
    await egressConnect.waitForResults();
    await egressConnect.connectFolder();

    const egressConfirm = new EgressConfirmationPage(page);
    await egressConfirm.confirmConnect();

    // Step 4: Connect NetApp folder
    const netappConnect = new NetAppConnectPage(page);
    await netappConnect.waitForFolders();
    await netappConnect.connectFolder();

    const netappConfirm = new NetAppConfirmationPage(page);
    await netappConfirm.confirmConnect();

    // Step 5: Navigate to Transfer Materials tab
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    // Step 6: Select all 3 files from Egress panel and initiate Copy
    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();

    // Wait for all 3 files to be indexed by Egress (may take time after upload)
    await transferTab.waitForFileCount(testData.files.length, "4. Served Evidence", 180_000);

    const fileIndices = testData.files.map((_, i) => i);
    await transferTab.selectEgressFiles(fileIndices);

    await transferTab.selectAction("Copy");

    // Step 7: Confirm transfer
    await transferTab.confirmTransfer();

    // Step 8: Wait for transfer to complete
    await transferTab.waitForTransferComplete(300_000);

    // Step 9: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy");

    // Step 10: Download CSV and verify
    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
