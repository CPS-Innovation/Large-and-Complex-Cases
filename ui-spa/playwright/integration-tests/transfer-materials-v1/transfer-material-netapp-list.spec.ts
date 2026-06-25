import { test } from "../utils/test";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";

test.describe("transfer material shared-drive list", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
  });

  test("Should show the transfer material tab with correct initial content", async ({
    page,
  }) => {
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.clickToggleTransferDirection();
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Shared Drive: Thunderstruck",
    ]);
    await transferMaterialsSourcePage.validateTableColumnHeaders();

    const folderRows = [
      ["", "netapp-folder-1-0", "--", "--"],
      ["", "netapp-folder-1-1", "--", "--"],
      ["", "netapp-file-1-0.pdf", "02/01/2000", "1.23 KB"],
    ];
    await transferMaterialsSourcePage.validateTableRowValues(folderRows);
  });

  test("Should correctly navigate through the shared-drive folders and validate checkbox visibility", async ({
    page,
  }) => {
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.clickToggleTransferDirection();
    await transferMaterialsSourcePage.verifySharedDriveTransferSourceElements();
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Shared Drive: Thunderstruck",
    ]);
    await transferMaterialsSourcePage.validateTableColumnHeaders();

    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "netapp-folder-1-0", "--", "--"],
      ["", "netapp-folder-1-1", "--", "--"],
      ["", "netapp-file-1-0.pdf", "02/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
    await transferMaterialsSourcePage.handleFolderClick("netapp-folder-1-0");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Shared Drive: Thunderstruck",
      "netapp-folder-1-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "netapp-folder-2-0", "--", "--"],
      ["", "netapp-folder-2-1", "--", "--"],
      ["", "netapp-file-2-0.pdf", "02/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
    await transferMaterialsSourcePage.handleFolderClick("netapp-folder-2-0");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Shared Drive: Thunderstruck",
      "netapp-folder-1-0",
      "netapp-folder-2-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "netapp-folder-3-0", "--", "--"],
      ["", "netapp-folder-3-1", "--", "--"],
      ["", "netapp-file-3-0.pdf", "02/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
    await transferMaterialsSourcePage.handleFolderClick("netapp-folder-3-0");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Shared Drive: Thunderstruck",
      "netapp-folder-1-0",
      "netapp-folder-2-0",
      "netapp-folder-3-0",
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(false, 1);
    await transferMaterialsSourcePage.verifyNoResults();

    await transferMaterialsSourcePage.handleFolderClick("netapp-folder-1-0");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Shared Drive: Thunderstruck",
      "netapp-folder-1-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "netapp-folder-2-0", "--", "--"],
      ["", "netapp-folder-2-1", "--", "--"],
      ["", "netapp-file-2-0.pdf", "02/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
    await transferMaterialsSourcePage.handleFolderClick(
      "Shared Drive: Thunderstruck",
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "shared-drive",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Shared Drive: Thunderstruck",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "netapp-folder-1-0", "--", "--"],
      ["", "netapp-folder-1-1", "--", "--"],
      ["", "netapp-file-1-0.pdf", "02/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
  });
});
