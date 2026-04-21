import { test, expect } from "../fixtures/test-fixtures-default";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";
import * as fs from "fs";
import * as path from "path";

test.describe("NetApp to Egress Copy (Default Mode)", () => {
  test("should copy files from NetApp to Egress using existing case", async ({
    page,
    testData,
  }) => {
    test.setTimeout(300_000);
    const { caseUrn } = testData;

    // Step 1: Search for case by URN
    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    // Step 2: Click View on already-connected case
    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    // Step 3: Navigate to Transfer Materials tab and wait for panels to load
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.waitForNetAppFiles();

    // Step 4: Switch to NetApp -> Egress direction
    await transferTab.switchToNetAppSource();

    // Step 5: Wait for NetApp files to load as source
    await transferTab.waitForNetAppFiles();

    // Step 6: Sort NetApp files by date descending so newest files appear first
    const dateHeader = page.getByTestId("netapp-table-wrapper").getByRole("button", { name: "Last modified date" });
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();
    await dateHeader.click();
    await transferTab.waitForNetAppFiles();

    // Step 7: Read the filename transferred to NetApp by the egress-to-netapp-copy-default test
    const sharedFile = path.resolve(__dirname, "../test-results/.last-transferred-file.txt");
    let targetFileName: string;
    if (fs.existsSync(sharedFile)) {
      targetFileName = fs.readFileSync(sharedFile, "utf-8").trim();
    } else {
      // Fallback: use own uploaded file name (may not be on NetApp yet)
      targetFileName = testData.files[0].fileName;
    }

    // Select the file by name from NetApp
    const netAppFileRow = page.getByTestId("netapp-table-wrapper")
      .locator("tbody tr", { hasText: targetFileName });
    await netAppFileRow.waitFor({ state: "visible", timeout: 30_000 });
    const checkbox = netAppFileRow.locator('input[type="checkbox"]');
    await checkbox.scrollIntoViewIfNeeded();
    await checkbox.check();

    // Step 8: Navigate into Egress destination subfolder
    // Use a different folder than where the file was uploaded (4. Served Evidence)
    await transferTab.navigateToFolder("2. Counsel only");
    await transferTab.waitForEgressFiles();

    // Step 9: Initiate Copy to Egress
    await transferTab.selectReverseAction("Copy");

    // Step 10: Confirm transfer
    await transferTab.confirmTransfer();

    // Step 10: Wait for transfer to complete
    await transferTab.waitForTransferComplete();

    // Step 11: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy");

    // Step 12: Download CSV and verify
    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  });
});
