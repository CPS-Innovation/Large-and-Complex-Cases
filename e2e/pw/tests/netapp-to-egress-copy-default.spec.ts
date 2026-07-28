import { test } from "../fixtures/test-fixtures-default";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { getTransferMaterialsTab } from "../pages/getTransferMaterialsTab";
import { TransferDestinationPage } from "../pages/TransferDestinationPage";
import { ActivityLogTab } from "../pages/ActivityLogTab";
import { loadEnvConfig } from "../helpers/env-config";
import { NETAPP_FIXTURE_FILENAME } from "../helpers/constants";

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

    const transferTab = getTransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.waitForNetAppFiles();

    // Switch to NetApp -> Egress direction.
    await transferTab.switchToNetAppSource();
    await transferTab.waitForNetAppFiles();

    // Select the canonical NetApp fixture file by exact name. Throws a
    // clear "fixture missing" error if not seeded. See README "Required
    // NetApp fixture" for setup.
    await transferTab.selectNetAppFileByExactName(NETAPP_FIXTURE_FILENAME);

    // Egress destination — different parent folder than upload source,
    // per-run timestamped subfolder so repeat runs never collide.
    if (loadEnvConfig().transferMaterialsV1) {
      // New screen: no second panel — Copy selected navigates to the
      // destination-tree page where the target folder is chosen.
      await transferTab.selectAction("Copy", "netAppToEgress");
      await new TransferDestinationPage(page).chooseFolder("Copy", [
        "2. Counsel only",
        uploadSubfolder!,
      ]);
    } else {
      // Old screen: navigate the Egress panel to the destination, then confirm.
      await transferTab.navigateToFolder("2. Counsel only");
      await transferTab.waitForEgressFiles();
      if (uploadSubfolder) {
        await transferTab.navigateToFolder(uploadSubfolder);
        await transferTab.waitForEgressFiles();
      }
      await transferTab.selectAction("Copy", "netAppToEgress");
      await transferTab.confirmTransfer("Copy");
    }
    await transferTab.waitForTransferComplete();

    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy", uploadSubfolder!);

    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
