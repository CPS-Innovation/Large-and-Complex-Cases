import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";
import { a } from "vitest/dist/chunks/suite.qtkXWc6R.js";

test.describe("transfer material egress list", () => {
  // validateFolderPath moved to shared fixture
  // Trigger the shared navigation fixture for every test in this suite so
  // individual tests don't need to include it in their signature.
  // test.beforeEach(async ({ openTransferMaterialsPage }) => {
  //   // destructuring the fixture triggers it; mark as used to satisfy linters
  //   openTransferMaterialsPage;
  // });
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
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

  test("Should correctly navigate through the egress folders", async ({
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
  });

  // test("Should show no results page if the egress folders return empty results", async ({
  //   page,
  //   worker,
  //   validateFolderPath,
  // }) => {
  //   await worker.use(
  //     http.get(
  //       "https://mocked-out-api/api/v1/egress/workspaces/egress_1/files",
  //       async () => {
  //         await delay(500);
  //         return HttpResponse.json({
  //           data: [],
  //           pagination: {
  //             totalResults: 50,
  //             skip: 0,
  //             take: 50,
  //             count: 25,
  //           },
  //         });
  //       },
  //     ),
  //   );
  //   await page.goto("/case/12/case-management");
  //   await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
  //   await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
  //   await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
  //   await expect(
  //     page.getByTestId("egress-folder-table-loader"),
  //   ).not.toBeVisible();
  //   await validateFolderPath(page, ["Home"]);

  //   await expect(
  //     page.getByTestId("egress-container").getByTestId("no-documents-text"),
  //   ).toBeVisible();
  //   await expect(
  //     page.getByTestId("egress-container").getByTestId("no-documents-text"),
  //   ).toHaveText("There are no documents currently in this folder");
  // });

  // test("Should hide checkboxes from the root egress folders and show checkboxes for the rest of the folders, when the egress is the source table", async ({
  //   page,
  //   validateFolderPath,
  // }) => {
  //   await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
  //   await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
  //   await expect(
  //     page.getByTestId("egress-folder-table-loader"),
  //   ).not.toBeVisible();
  //   const egressTableWrapper = page.getByTestId("egress-table-wrapper");
  //   await validateFolderPath(page, ["Home"]);
  //   const checkboxes = await egressTableWrapper
  //     .locator('table input[type="checkbox"]')
  //     .all();

  //   await Promise.all(
  //     checkboxes.map((checkbox) => expect(checkbox).toBeHidden()),
  //   );
  //   await egressTableWrapper.locator('role=button[name="folder-1-0"]').click();
  //   await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
  //   await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
  //   await expect(
  //     page.getByTestId("egress-folder-table-loader"),
  //   ).not.toBeVisible();
  //   await validateFolderPath(page, ["Home", "folder-1-0"]);
  //   await Promise.all(
  //     checkboxes.map((checkbox) => expect(checkbox).not.toBeHidden()),
  //   );
  //   await egressTableWrapper.locator('role=button[name="folder-2-0"]').click();
  //   await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
  //   await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
  //   await expect(
  //     page.getByTestId("egress-folder-table-loader"),
  //   ).not.toBeVisible();
  //   await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-0"]);
  //   await Promise.all(
  //     checkboxes.map((checkbox) => expect(checkbox).not.toBeHidden()),
  //   );
  //   await egressTableWrapper.locator('role=button[name="Home"]').click();
  //   await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
  //   await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
  //   await validateFolderPath(page, ["Home"]);
  //   await expect(
  //     page.getByTestId("egress-folder-table-loader"),
  //   ).not.toBeVisible();
  //   await Promise.all(
  //     checkboxes.map((checkbox) => expect(checkbox).toBeHidden()),
  //   );
  // });

  // test("Should hide checkboxes from the table head, if there are not contents inside the folder, when the egress is the source table", async ({
  //   page,
  //   validateFolderPath,
  // }) => {
  //   await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
  //   await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
  //   await expect(
  //     page.getByTestId("egress-folder-table-loader"),
  //   ).not.toBeVisible();
  //   const egressTableWrapper = page.getByTestId("egress-table-wrapper");
  //   await validateFolderPath(page, ["Home"]);
  //   const checkboxes = await egressTableWrapper
  //     .locator('table input[type="checkbox"]')
  //     .all();

  //   await Promise.all(
  //     checkboxes.map((checkbox) => expect(checkbox).toBeHidden()),
  //   );
  //   await egressTableWrapper.locator('role=button[name="folder-1-0"]').click();
  //   await validateFolderPath(page, ["Home", "folder-1-0"]);
  //   await egressTableWrapper.locator('role=button[name="folder-2-0"]').click();
  //   await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-0"]);
  //   await egressTableWrapper.locator('role=button[name="folder-3-0"]').click();
  //   await validateFolderPath(page, [
  //     "Home",
  //     "folder-1-0",
  //     "folder-2-0",
  //     "folder-3-0",
  //   ]);

  //   await expect(
  //     page.getByTestId("egress-container").getByTestId("no-documents-text"),
  //   ).toBeVisible();
  //   await expect(
  //     page.getByTestId("egress-container").getByTestId("no-documents-text"),
  //   ).toHaveText("There are no documents currently in this folder");
  // });
});
