import { test } from "../fixtures/test-fixtures-default-large";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("Egress to NetApp Copy - Large File 200MB (Default Mode)", () => {
  test("should copy a 200MB file from Egress to NetApp using existing case", async ({
    page,
    testData,
  }) => {
    test.setTimeout(900_000);
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

    // Step 4: Select files from Egress panel and initiate Copy
    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    // Wait for the uploaded file to be indexed by Egress. Reload +
    // re-navigate retry — plain waitFor spins against a stale DOM.
    const fileName = testData.files[0].fileName;
    const sourceFolderPath = uploadSubfolder
      ? ["4. Served Evidence", uploadSubfolder]
      : ["4. Served Evidence"];
    await transferTab.waitForEgressFileByName(fileName, sourceFolderPath);

    // Select the just-uploaded file by name to avoid picking old files already on NetApp
    await transferTab.selectEgressFileByName(fileName);

    await transferTab.selectAction("Copy");

    // Step 5: Confirm transfer
    await transferTab.confirmTransfer();

    // Step 6: Wait for transfer to complete (5 min timeout for large file)
    await transferTab.waitForTransferComplete(300_000);

    // Step 7: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy", uploadSubfolder!);

    // Step 8: Download CSV and verify
    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
