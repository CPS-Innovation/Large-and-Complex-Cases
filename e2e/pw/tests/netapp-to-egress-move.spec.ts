import { test } from "../fixtures/test-fixtures-register-case";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

// Move is only exposed on the NetApp -> Egress direction and is gated by
// feature flag + Azure AD group (PRIVATE_BETA_FEATURE_USER_GROUP2). See
// useUserGroupsFeatureFlag.ts. The Egress -> NetApp panel has no Move
// button at all.

test.describe("NetApp to Egress Move", () => {
  test.use({ testOptions: { fileSizeMb: 100, fileCount: 1 } });

  test("should move files from NetApp to Egress", async ({
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

    await transferTab.switchToNetAppSource();
    await transferTab.waitForNetAppFiles();

    const dateHeader = page
      .getByTestId("netapp-table-wrapper")
      .getByRole("button", { name: "Last modified date" });
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();

    await transferTab.selectNetAppFiles([0]);

    await transferTab.navigateToFolder("2. Counsel only");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    await transferTab.selectReverseAction("Move");
    await transferTab.confirmTransfer();
    await transferTab.waitForTransferComplete();

    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Move");

    await activityLog.expandFileList("Move");
    await activityLog.downloadCsv("Move");
    await activityLog.verifyDownloadSuccess();
  });
});
