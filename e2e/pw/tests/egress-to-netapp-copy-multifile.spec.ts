import { test } from "../fixtures/test-fixtures-register-case";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("Egress to NetApp Copy - Multiple Files (50MB x 3)", () => {
  test.use({ testOptions: { fileSizeMb: 50, fileCount: 3 } });

  test("should copy 3 files from Egress to NetApp", async ({
    page,
    testData,
  }) => {
    test.setTimeout(600_000);
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

    // Step 3: Select this test's uploaded files by name and initiate Copy
    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    // Wait for the last-uploaded file to be indexed. Reload + re-navigate
    // on each retry — Egress doesn't auto-refresh the file list.
    const lastFileName = testData.files[testData.files.length - 1].fileName;
    await transferTab.waitForEgressFileByName(
      lastFileName,
      uploadSubfolder
        ? ["4. Served Evidence", uploadSubfolder]
        : ["4. Served Evidence"]
    );

    for (const file of testData.files) {
      await transferTab.selectEgressFileByName(file.fileName);
    }

    await transferTab.selectAction("Copy");

    // Step 4: Confirm and wait for completion
    await transferTab.confirmTransfer();
    await transferTab.waitForTransferComplete(300_000);

    // Step 5: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy");

    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
