import { test, expect } from "../fixtures/test-fixtures-register-case";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";

test.describe("Full-flow smoke", () => {
  test.use({ testOptions: { fileSizeMb: 100, fileCount: 1 } });

  test("select-all Egress->NetApp copy verifies file lands in NetApp panel", async ({
    page,
    testData,
  }) => {
    test.setTimeout(300_000);
    const { caseUrn, files, uploadSubfolder } = testData;

    // Step 1: Search for pre-connected case (searchByUrn does goto + form fill)
    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    // Step 2: Transfer Materials, select all in Served Evidence, copy
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();

    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }
    // Wait for the uploaded file to be indexed before select-all (the
    // header checkbox is hidden while the table is empty). Egress doesn't
    // auto-refresh, so plain waitFor spins against a stale DOM — the
    // helper reloads + re-navigates on each retry.
    await transferTab.waitForEgressFileByName(
      files[0].fileName,
      uploadSubfolder
        ? ["4. Served Evidence", uploadSubfolder]
        : ["4. Served Evidence"]
    );
    await transferTab.selectAllEgressFiles();
    await transferTab.selectAction("Copy");
    await transferTab.confirmTransfer();
    await transferTab.waitForTransferComplete();

    // Step 3: Verify file shows up in NetApp destination panel
    const fileName = files[0].fileName;
    const netappTable = page.getByTestId("netapp-table-wrapper");
    await expect(netappTable).toBeVisible({ timeout: 30_000 });
    await expect(netappTable).toContainText(fileName, { timeout: 30_000 });

    // Step 4: Verify Activity Log entry
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Copy", uploadSubfolder!);
  });
});
