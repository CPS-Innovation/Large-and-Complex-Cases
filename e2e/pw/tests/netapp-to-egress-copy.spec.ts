import { test, expect } from "../fixtures/test-fixtures";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { EgressConnectPage } from "../pages/EgressConnectPage";
import { EgressConfirmationPage } from "../pages/EgressConfirmationPage";
import { NetAppConnectPage } from "../pages/NetAppConnectPage";
import { NetAppConfirmationPage } from "../pages/NetAppConfirmationPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("NetApp to Egress Copy", () => {
  test("should copy files from NetApp to Egress", async ({
    page,
    testData,
  }) => {
    test.setTimeout(300_000);
    const { caseUrn, workspace } = testData;

    // Step 1: Search for case by URN
    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    // Step 2: Click into case from results
    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    // Step 3: Connect Egress workspace
    const egressConnect = new EgressConnectPage(page);
    await egressConnect.searchFolder(workspace.name);
    await egressConnect.waitForResults();
    await egressConnect.connectFolder();

    const egressConfirm = new EgressConfirmationPage(page);
    await egressConfirm.confirmConnect();

    // Step 4: Connect NetApp folder
    const netappConnect = new NetAppConnectPage(page);
    await netappConnect.waitForFolders();
    await netappConnect.connectFolder();

    const netappConfirm = new NetAppConfirmationPage(page);
    await netappConfirm.confirmConnect();

    // Step 5: Navigate to Transfer Materials tab and wait for panels to load
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.waitForNetAppFiles();

    // Step 6: Switch to NetApp -> Egress direction
    await transferTab.switchToNetAppSource();

    // Step 7: Wait for NetApp files to load as source
    await transferTab.waitForNetAppFiles();

    // Step 8: Select the latest file from NetApp source panel
    await transferTab.selectNetAppFiles([0]);

    // Step 9: Navigate into Egress destination subfolder
    // (egress-inset-text only appears when navigated past root folder)
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();

    // Step 10: Initiate Copy to Egress
    await transferTab.selectReverseAction("Copy");

    // Step 11: Confirm transfer
    await transferTab.confirmTransfer();

    // Step 12: Wait for transfer to complete
    await transferTab.waitForTransferComplete();

    // Step 13: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy");

    // Step 14: Download CSV and verify
    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
