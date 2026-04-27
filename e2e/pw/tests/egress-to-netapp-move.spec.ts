import { test } from "../fixtures/test-fixtures-register-case";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("Egress to NetApp Move", () => {
  test.use({ testOptions: { fileSizeMb: 100, fileCount: 1 } });

  test("should move files from Egress to NetApp", async ({
    page,
    testData,
  }) => {
    test.setTimeout(300_000);
    const { caseUrn, uploadSubfolder } = testData;

    // Step 1: Search for pre-connected case
    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    // Step 2: Navigate to Transfer Materials tab
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    // Step 3: Select this test's uploaded files by name and initiate Move
    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    for (const file of testData.files) {
      await transferTab.selectEgressFileByName(file.fileName);
    }

    await transferTab.selectAction("Move");

    // Step 4: Confirm and wait for completion
    await transferTab.confirmTransfer();
    await transferTab.waitForTransferComplete();

    // Step 5: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Move");

    await activityLog.expandFileList("Move");
    await activityLog.downloadCsv("Move");
    await activityLog.verifyDownloadSuccess();
  });
});
