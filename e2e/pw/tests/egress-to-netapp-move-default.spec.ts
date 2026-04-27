import { test } from "../fixtures/test-fixtures-default";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("Egress to NetApp Move (Default Mode)", () => {
  test.skip("should move files from Egress to NetApp using existing case", async ({
    page,
    testData,
  }) => {
    test.setTimeout(300_000);
    const { caseUrn, uploadSubfolder } = testData;

    // Step 1: Search for case by URN
    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    // Step 2: Click View on already-connected case
    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    // Step 3: Navigate to Transfer Materials tab
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    // Step 4: Select files from Egress panel and initiate Move
    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    const fileIndices = testData.files.map((_, i) => i);
    await transferTab.selectEgressFiles(fileIndices);

    await transferTab.selectAction("Move");

    // Step 5: Confirm transfer
    await transferTab.confirmTransfer();

    // Step 6: Wait for transfer to complete
    await transferTab.waitForTransferComplete();

    // Step 7: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Move");

    // Step 8: Download CSV and verify
    await activityLog.expandFileList("Move");
    await activityLog.downloadCsv("Move");
    await activityLog.verifyDownloadSuccess();
  });
});
