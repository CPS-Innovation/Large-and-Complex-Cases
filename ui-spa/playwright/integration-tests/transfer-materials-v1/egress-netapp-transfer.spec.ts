import { test } from "../utils/test";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";
import { TransferMaterialsDestinationPage } from "../pages/transfer-material-destination";
test.describe("transfer material egress list", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
  });

  test("Should successfully handle the copy of materials from egress to shared drive", async ({
    page,
  }) => {
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.verifyEgressTransferSourceElements();
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Egress: Thunderstruck",
    ]);
    await transferMaterialsSourcePage.validateTableColumnHeaders();

    const folderRows = [
      ["", "folder-1-0", "02/01/2000", "--"],
      ["", "folder-1-1", "03/01/2000", "--"],
      ["", "file-1-2.pdf", "03/01/2000", "1.23 KB"],
    ];
    await transferMaterialsSourcePage.validateTableRowValues(folderRows);
    await transferMaterialsSourcePage.verifyCopyBtnEnabled(false);
    await transferMaterialsSourcePage.verifyMoveBtnEnabled(false);

    await transferMaterialsSourcePage.handleFolderClick("folder-1-0");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Egress: Thunderstruck",
      "folder-1-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "folder-2-0", "02/01/2000", "--"],
      ["", "folder-2-1", "03/01/2000", "--"],
      ["", "file-2-2.pdf", "03/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
    await transferMaterialsSourcePage.verifyCopyBtnEnabled(false);
    await transferMaterialsSourcePage.verifyMoveBtnEnabled(false);
    await transferMaterialsSourcePage.toggleCheckbox(0);
    await transferMaterialsSourcePage.verifyCopyBtnEnabled(true);
    await transferMaterialsSourcePage.verifyMoveBtnEnabled(true);
    await transferMaterialsSourcePage.clickCopyBtn();

    const transferMaterialsDestinationPage =
      new TransferMaterialsDestinationPage(page);
    await transferMaterialsDestinationPage.verifyUrl(
      "/case/12/case-management/transfer-destination-page",
    );
    await transferMaterialsDestinationPage.verifyPageElements(
      "egress",
      3,
      "copy",
    );
    await transferMaterialsDestinationPage.verifyFolderExpanded(
      "Shared Drive: Thunderstruck",
      true,
      ["netapp-folder-1-0", "netapp-folder-1-1"],
    );
    await transferMaterialsDestinationPage.clickMinimizeFolder(
      "Shared Drive: Thunderstruck",
    );
    await transferMaterialsDestinationPage.verifyFolderExpanded(
      "Shared Drive: Thunderstruck",
      false,
      [],
    );
    await transferMaterialsDestinationPage.clickExpandFolder(
      "Shared Drive: Thunderstruck",
    );
    await transferMaterialsDestinationPage.verifyFolderExpanded(
      "Shared Drive: Thunderstruck",
      true,
      ["netapp-folder-1-0", "netapp-folder-1-1"],
    );
    await transferMaterialsDestinationPage.clickExpandFolder(
      "netapp-folder-1-0",
    );
    await transferMaterialsDestinationPage.verifyTransferDestinationTableLoader(
      true,
    );
    await transferMaterialsDestinationPage.verifyTransferDestinationTableLoader(
      false,
    );
    await transferMaterialsDestinationPage.verifyFolderExpanded(
      "netapp-folder-1-0",
      true,
      ["netapp-folder-2-0", "netapp-folder-2-1"],
    );
    await transferMaterialsDestinationPage.clickMinimizeFolder(
      "netapp-folder-1-0",
    );
    await transferMaterialsDestinationPage.verifyFolderExpanded(
      "netapp-folder-1-0",
      false,
    );
    await transferMaterialsDestinationPage.verifyTransferActionEnabled(false);
    await transferMaterialsDestinationPage.selectFolder("netapp-folder-1-0");
    await transferMaterialsDestinationPage.verifyTransferActionEnabled(true);
    await transferMaterialsDestinationPage.verifyTransferActionButtonName(
      "Copy to netapp-folder-1-0",
    );
    await transferMaterialsDestinationPage.clickTransferActionButton();
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.validateTransferSuccessBanner([
      {
        folderPath: "folder2",
        files: ["file1.txt", "file2.txt"],
      },
      {
        folderPath: "folder3",
        files: ["file3.txt"],
      },
    ]);
  });
});
