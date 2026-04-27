import { test } from "../fixtures/test-fixtures-default";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("NetApp to Egress Copy (Default Mode)", () => {
  test("should copy files from NetApp to Egress using existing case", async ({
    page,
    testData,
  }) => {
    test.setTimeout(300_000);
    const { caseUrn, uploadSubfolder } = testData;

    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.waitForNetAppFiles();

    // Switch to NetApp -> Egress direction.
    await transferTab.switchToNetAppSource();
    await transferTab.waitForNetAppFiles();

    // Sort by Last modified date (two clicks intended to land descending).
    // We don't validate the sort because some accumulated NetApp content
    // is fine as a source — the per-run timestamped destination subfolder
    // means any file copies into a unique location.
    const dateHeader = page
      .getByTestId("netapp-table-wrapper")
      .getByRole("button", { name: "Last modified date" });
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();

    await transferTab.selectNetAppFiles([0]);

    // Egress destination — different parent folder than the upload source,
    // then per-run subfolder so repeat runs never collide on destination.
    await transferTab.navigateToFolder("2. Counsel only");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    await transferTab.selectReverseAction("Copy");
    await transferTab.confirmTransfer();
    await transferTab.waitForTransferComplete();

    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy");

    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
