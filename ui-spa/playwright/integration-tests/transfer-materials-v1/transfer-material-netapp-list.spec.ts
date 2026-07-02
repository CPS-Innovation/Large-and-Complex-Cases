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
      ["", "folder-1-0", "--", "--"],
      ["", "folder-1-1", "--", "--"],
      ["", "file-1-0.pdf", "02/01/2000", "1.23 KB"],
      ["", "file-1-1.pdf", "03/01/2000", "2.26 MB"],
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
      ["", "folder-1-0", "--", "--"],
      ["", "folder-1-1", "--", "--"],
      ["", "file-1-0.pdf", "02/01/2000", "1.23 KB"],
      ["", "file-1-1.pdf", "03/01/2000", "2.26 MB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 5);
    await transferMaterialsSourcePage.handleFolderClick("folder-1-0");
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
      "folder-1-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "folder-2-0", "--", "--"],
      ["", "folder-2-1", "--", "--"],
      ["", "file-2-0.pdf", "02/01/2000", "1.23 KB"],
      ["", "file-2-1.pdf", "03/01/2000", "2.26 MB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 5);
    await transferMaterialsSourcePage.handleFolderClick("folder-2-0");
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
      "folder-1-0",
      "folder-2-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "folder-3-0", "--", "--"],
      ["", "folder-3-1", "--", "--"],
      ["", "file-3-0.pdf", "02/01/2000", "1.23 KB"],
      ["", "file-3-1.pdf", "03/01/2000", "2.26 MB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 5);
    await transferMaterialsSourcePage.handleFolderClick("folder-3-0");
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
      "folder-1-0",
      "folder-2-0",
      "folder-3-0",
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(false, 1);
    await transferMaterialsSourcePage.verifyNoResults();

    await transferMaterialsSourcePage.handleFolderClick("folder-1-0");
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
      "folder-1-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "folder-2-0", "--", "--"],
      ["", "folder-2-1", "--", "--"],
      ["", "file-2-0.pdf", "02/01/2000", "1.23 KB"],
      ["", "file-2-1.pdf", "03/01/2000", "2.26 MB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 5);
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
      ["", "folder-1-0", "--", "--"],
      ["", "folder-1-1", "--", "--"],
      ["", "file-1-0.pdf", "02/01/2000", "1.23 KB"],
      ["", "file-1-1.pdf", "03/01/2000", "2.26 MB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 5);
  });
});
