import { test } from "../fixtures/test-fixtures-register-case";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { ActivityLogTab } from "../pages/ActivityLogTab";
import { verifyNetAppFileSizeByName, isFileInEgress } from "../helpers/transfer-verify";
import { expect } from "@playwright/test";

test.describe("Egress to NetApp Move", () => {
  test.use({ testOptions: { fileSizeMb: 100, fileCount: 1 } });

  test("should move files from Egress to NetApp", async ({
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

    // Step 2: Navigate to Transfer Materials tab
    const caseMgmt = new CaseManagementPage(page);
    await caseMgmt.waitForLoad();
    await caseMgmt.switchToTab("transfer-materials");

    // Step 3: Select this test's uploaded files by name and initiate Move
    const transferTab = new TransferMaterialsTab(page);
    await transferTab.waitForEgressFiles();
    await transferTab.navigateToFolder("4. Served Evidence");
    await transferTab.waitForEgressFiles();
    if (uploadSubfolder) {
      await transferTab.navigateToFolder(uploadSubfolder);
      await transferTab.waitForEgressFiles();
    }

    for (const file of testData.files) {
      await transferTab.selectEgressFileByName(file.fileName);
    }

    await transferTab.selectAction("Move");

    // Step 4: Confirm and wait for completion
    await transferTab.confirmTransfer();
    await transferTab.waitForTransferComplete();

    // Step 5: Verify in Activity Log
    await caseMgmt.switchToTab("activity-log");
    const activityLog = new ActivityLogTab(page);
    await activityLog.waitForLogs();
    await activityLog.verifyTransferLogged("Move", uploadSubfolder!);

    await activityLog.expandFileList();
    await activityLog.downloadCsv();
    await activityLog.verifyDownloadSuccess();

    // Step 6: Confirm complete files exist in shared drive
    for (const file of testData.files) {
      console.log(`\nVerifying file '${file.fileName}' exists in NetApp in its original size (${file.fileSize} bytes)`)
      await verifyNetAppFileSizeByName(
        file.fileName,
        testData.caseId!,
        file.fileSize,
        "Automation-Testing",
      );
    }

    // Step 7: Confirm files removed from Egress
    for (const file of testData.files) {
      console.log(`\nVerifying file '${file.fileName}' has been deleted from source '${testData.uploadPath}'.`)
      await test.step(
        `Verify file '${file.fileName}' is no longer present in Egress`,
        async () => {
          const exists = await isFileInEgress(
            testData.workspace.id,
            testData.sourceSubfolderId!,
            file.fileName,
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
