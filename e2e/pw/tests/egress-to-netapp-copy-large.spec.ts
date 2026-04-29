import { test } from "../fixtures/test-fixtures-register-case";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("Egress to NetApp Copy - Large File (200MB)", () => {
  test.use({ testOptions: { fileSizeMb: 200, fileCount: 1 } });

  test("should copy a 200MB file from Egress to NetApp", async ({
    page,
    testData,
  }) => {
    test.setTimeout(900_000);
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

    // Step 3: Select this test's uploaded file by name and initiate Copy
    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    // Wait for the uploaded file to be indexed by Egress (may take time after large upload)
    const fileName = testData.files[0].fileName;
    const egressTable = page.getByTestId("egress-table-wrapper");
    await egressTable.locator("tbody tr", { hasText: fileName }).waitFor({
      state: "visible",
      timeout: 180_000,
    });

    await transferTab.selectEgressFileByName(fileName);
    await transferTab.selectAction("Copy");

    // Step 4: Confirm and wait for completion (5 min timeout for large file)
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
