import { delay, HttpResponse, http } from "msw";
import { test } from "../utils/test";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";

test.describe("transfer material egress list", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management?transfer-materials-v1=true");
  });

  test("Should show the transfer material tab with correct initial content", async ({
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
  });

  test("Should correctly navigate through the egress folders and validate checkbox visibility", async ({
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

    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "folder-1-0", "02/01/2000", "--"],
      ["", "folder-1-1", "03/01/2000", "--"],
      ["", "file-1-2.pdf", "03/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(false, 4);
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
    await transferMaterialsSourcePage.handleFolderClick("folder-2-0");
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
      "folder-2-0",
    ]);
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "folder-3-0", "02/01/2000", "--"],
      ["", "folder-3-1", "03/01/2000", "--"],
      ["", "file-3-2.pdf", "03/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
    await transferMaterialsSourcePage.handleFolderClick("folder-3-0");
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
      "folder-2-0",
      "folder-3-0",
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(false, 1);
    await transferMaterialsSourcePage.verifyNoResults();

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
    await transferMaterialsSourcePage.handleFolderClick(
      "Egress: Thunderstruck",
    );
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
    await transferMaterialsSourcePage.validateTableRowValues([
      ["", "folder-1-0", "02/01/2000", "--"],
      ["", "folder-1-1", "03/01/2000", "--"],
      ["", "file-1-2.pdf", "03/01/2000", "1.23 KB"],
    ]);
    await transferMaterialsSourcePage.verifyCheckboxesVisibility(false, 4);
  });
  test("Should show the leadDefendant name in the Home path for Egress if the operation name is null", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: 12,
          egressWorkspaceId: "egress_1",
          netappFolderPath: "netapp/",
          operationName: null,
          leadDefendantName: "John Doe",
          urn: "45AA2098221",
          activeTransferId: "",
        });
      }),
    );
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyPageElements("John Doe");
    await transferMaterialsSourcePage.verifyEgressTransferSourceElements();
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath(["Egress: John Doe"]);
  });
});
