import { test } from "../fixtures/test-fixtures-register-case";
import { loadEnvConfig } from "../helpers/env-config";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { getTransferMaterialsTab } from "../pages/getTransferMaterialsTab";
import { TransferDestinationPage } from "../pages/TransferDestinationPage";
import { ActivityLogTab } from "../pages/ActivityLogTab";
import { isFileInEgress } from "../helpers/transfer-verify";
import { expect } from "@playwright/test";

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

    const transferTab = getTransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.waitForNetAppFiles();

    // Step 3: Switch to NetApp -> Egress direction
    await transferTab.switchToNetAppSource();
    await transferTab.waitForNetAppFiles();

    // Step 4: Sort NetApp by date, select row 0.
    // Pre-condition: REGISTER_CASE_NETAPP_FOLDER must contain >=1 file.
    // Source identity doesn't matter — destination is a fresh per-run
    // workspace, no collisions. See README "NetApp source pre-condition".
    await transferTab.sortNetAppByDateDescending();

    await transferTab.selectNetAppFiles([0]);

    // Step 5+6: Initiate Copy to an Egress destination subfolder. The
    // destination is "2. Counsel only" -> this run's dated subfolder (a
    // different top-level folder than the "4. Served Evidence" source, so
    // repeat runs don't collide).
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

    // Step 7: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy", uploadSubfolder!);

    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();

    // No further verification added as currently we're not using a predictable file name
  });
});
