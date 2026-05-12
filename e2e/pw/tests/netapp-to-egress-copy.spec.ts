import { test } from "../fixtures/test-fixtures-register-case";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("NetApp to Egress Copy", () => {
  test.use({ testOptions: { fileSizeMb: 100, fileCount: 1 } });

  test("should copy files from NetApp to Egress", async ({
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

    // Step 2: Navigate to Transfer Materials tab and wait for panels to load
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.waitForNetAppFiles();

    // Step 3: Switch to NetApp -> Egress direction
    await transferTab.switchToNetAppSource();
    await transferTab.waitForNetAppFiles();

    // Step 4: Sort NetApp by date, select row 0.
    // Pre-condition: REGISTER_CASE_NETAPP_FOLDER must contain >=1 file.
    // Source identity doesn't matter — destination is a fresh per-run
    // workspace, no collisions. See README "NetApp source pre-condition".
    const dateHeader = page
      .getByTestId("netapp-table-wrapper")
      .getByRole("button", { name: "Last modified date" });
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();

    await transferTab.selectNetAppFiles([0]);

    // Step 5: Navigate into Egress destination subfolder.
    // Use a different top-level folder than the upload source (4. Served
    // Evidence), then into this test's dated subfolder so repeat runs don't
    // collide with files already copied there on previous runs.
    await transferTab.navigateToFolder("2. Counsel only");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    // Step 6: Initiate Copy to Egress
    await transferTab.selectReverseAction("Copy");
    await transferTab.confirmTransfer();
    await transferTab.waitForTransferComplete();

    // Step 7: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy", uploadSubfolder!);

    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
