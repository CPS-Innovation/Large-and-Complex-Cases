import { test } from "../fixtures/test-fixtures-default";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { getTransferMaterialsTab } from "../pages/getTransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";
import {
  verifyNetAppFileSizeByName,
  isFileInEgress,
  waitForFileInEgress,
} from "../helpers/transfer-verify";
import { expect } from "@playwright/test";

test.describe("Egress to NetApp Move (Default Mode)", () => {
  test("should move files from Egress to NetApp using existing case", async ({
    page,
    testData,
  }) => {
    // 30 min total: a 2GB file can take several minutes to index in Egress
    // (API-gated below) plus up to 10 min for the move itself.
    test.setTimeout(1_800_000);
    const { caseUrn, uploadSubfolder } = testData;

    // Step 1: Search for case by URN
    const caseSearch = new CaseSearchPage(page);
    await caseSearch.searchByUrn(caseUrn);

    // Step 2: Click View on already-connected case
    const searchResults = new SearchResultsPage(page);
    await searchResults.waitForResults();
    await searchResults.clickCaseAction(caseUrn);

    // Step 3: Navigate to Transfer Materials tab
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    // Step 4: Select files from Egress panel and initiate Move
    const transferTab = getTransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    // Gate on the API listing first: a 2GB file is confirmed via the uploads
    // endpoint before Egress finishes processing it into the workspace listing
    // the UI reads, so wait (cheaply, no browser) for it to actually appear in
    // the folder before asking the panel to render it.
    const lastFileName = testData.files[testData.files.length - 1].fileName;
    await waitForFileInEgress(
      testData.workspace.id,
      testData.sourceSubfolderId!,
      lastFileName,
      { timeoutMs: 720_000, egressToken: testData.egressToken! },
    );

    // Now that Egress lists the file, the panel only needs a reload to show it.
    const sourceFolderPath = uploadSubfolder
      ? ["4. Served Evidence", uploadSubfolder]
      : ["4. Served Evidence"];
    await transferTab.waitForEgressFileByName(
      lastFileName,
      sourceFolderPath,
      180_000,
    );

    // Select by name (not index) so we don't pick a stranger's old file
    // that happened to land at row 0.
    for (const file of testData.files) {
      await transferTab.selectEgressFileByName(file.fileName);
    }

    await transferTab.selectAction("Move");

    // Step 5: Confirm transfer
    await transferTab.confirmTransfer("Move");

    // Step 6: Wait for transfer to complete (10 min timeout)
    await transferTab.waitForTransferComplete(600_000); 

    // Step 7: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Move", uploadSubfolder!);

    // Step 8: Download CSV and verify
    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();
  
    // Step 9: Confirm complete files exist in shared drive
    for (const file of testData.files) {
      console.log(`\nVerifying file '${file.fileName}' exists in NetApp in its original size (${file.fileSize} bytes)`)
      await verifyNetAppFileSizeByName(
        file.fileName,
        testData.caseId!,
        file.fileSize,
      );
    }

    // Step 10: Confirm files removed from Egress
    for (const file of testData.files) {
      console.log(`\nVerifying file '${file.fileName}' has been deleted from source '${testData.uploadPath}'.`)
      await test.step(
        `Verify file '${file.fileName}' is no longer present in Egress`,
        async () => {
          const exists = await isFileInEgress(
            testData.workspace.id,
            testData.sourceSubfolderId!,
            file.fileName,
            testData.egressToken!,
          );

          expect(
            exists,
            `File '${file.fileName}' still exists in Egress`
          ).toBeFalsy();
        }
      );
    }
  });
});
