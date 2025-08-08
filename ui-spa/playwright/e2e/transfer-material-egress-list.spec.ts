import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";

test.describe("transfer material egress list", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer between Egress and Shared Drive");
  });
  const validateFolderPath = async (page, expectedResult: string[]) => {
    const texts = await page
      .getByTestId("egress-table-wrapper")
      .locator("ol>li")
      .allTextContents();
    expect(texts).toEqual(expectedResult);
  };

  test("Should show the transfer material tab with correct initial content", async ({
    page,
  }) => {
    await expect(page.getByText("45AA2098221")).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText(
      "Select the files or folders you want to transfer and where you want to put them.",
    );

    await expect(page.getByTestId("egress-container")).toBeVisible();
    await expect(page.getByTestId("netapp-container")).toBeVisible();
    await expect(
      page
        .getByTestId("egress-container")
        .locator("h3", { hasText: "Egress" }),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("netapp-container")
        .locator("h3", { hasText: "Shared drive" }),
    ).toBeVisible();
  });

  test("Should show the egress folders results correctly when egress is the source table", async ({
    page,
  }) => {
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    const egressTableWrapper = page.getByTestId("egress-table-wrapper");
    await validateFolderPath(page, ["Home"]);
    const tableHeadValues = await egressTableWrapper
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual([
      "",
      " Folder/file name",
      " Last modified date",
      " Size",
    ]);
    const row1Values = await egressTableWrapper
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual(["", "folder-1-0", "02/01/2000", "--"]);
    const row2Values = await egressTableWrapper
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual(["", "folder-1-1", "03/01/2000", "--"]);

    const row3Values = await egressTableWrapper
      .locator("table tbody tr:nth-child(3) td")
      .allTextContents();
    expect(row3Values).toEqual(["", "file-1-2.pdf", "03/01/2000", "1.23 KB"]);
  });

  test("Should show the egress folders results correctly when egress is the destination table", async ({
    page,
  }) => {
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await page.getByRole("button", { name: "from the Shared Drive to Egress" }).click();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer between Shared Drive and Egress");
    const egressTableWrapper = page.getByTestId("egress-table-wrapper");
    await validateFolderPath(page, ["Home"]);
    const tableHeadValues = await egressTableWrapper
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual([" Folder/file name", " Size"]);
    const row1Values = await egressTableWrapper
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual(["folder-1-0", "--"]);
    const row2Values = await egressTableWrapper
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual(["folder-1-1", "--"]);

    const row3Values = await egressTableWrapper
      .locator("table tbody tr:nth-child(3) td")
      .allTextContents();
    expect(row3Values).toEqual(["file-1-2.pdf", "1.23 KB"]);
  });

  test("Should correctly navigate through the egress folders", async ({
    page,
  }) => {
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    const egressTableWrapper = page.getByTestId("egress-table-wrapper");
    await validateFolderPath(page, ["Home"]);
    await egressTableWrapper.locator('role=button[name="folder-1-0"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0"]);
    await egressTableWrapper.locator('role=button[name="folder-2-0"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-0"]);
    await egressTableWrapper.locator('role=button[name="folder-3-0"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await egressTableWrapper.locator('role=button[name="folder-1-0"]').click();
    await validateFolderPath(page, ["Home", "folder-1-0"]);
    await egressTableWrapper.locator('role=button[name="folder-2-1"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-1"]);
    await egressTableWrapper.locator('role=button[name="folder-3-1"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, [
      "Home",
      "folder-1-0",
      "folder-2-1",
      "folder-3-1",
    ]);
    await egressTableWrapper.locator('role=button[name="folder-2-1"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-1"]);
    await expect(page.locator('role=button[name="folder-3-0"]')).toBeVisible();
    await expect(page.locator('role=button[name="folder-3-1"]')).toBeVisible();
    await egressTableWrapper.locator('role=button[name="Home"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await validateFolderPath(page, ["Home"]);
    await expect(page.locator('role=button[name="folder-1-0"]')).toBeVisible();
    await expect(page.locator('role=button[name="folder-1-1"]')).toBeVisible();
  });

  test("Should show no results page if the egress folders return empty results", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/egress/workspaces/egress_1/files",
        async () => {
          await delay(500);
          return HttpResponse.json({
            data: [],
            pagination: {
              totalResults: 50,
              skip: 0,
              take: 50,
              count: 25,
            },
          });
        },
      ),
    );
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home"]);
    await expect(
      page
        .getByTestId("egress-container")
        .getByText("There are no documents currently in this folder"),
    ).toBeVisible();
  });

  test("Should hide checkboxes from the root egress folders and show checkboxes for the rest of the folders, when the egress is the source table", async ({
    page,
  }) => {
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    const egressTableWrapper = page.getByTestId("egress-table-wrapper");
    await validateFolderPath(page, ["Home"]);
    const checkboxes = await egressTableWrapper
      .locator('table input[type="checkbox"]')
      .all();

    await Promise.all(
      checkboxes.map((checkbox) => expect(checkbox).toBeHidden()),
    );
    await egressTableWrapper.locator('role=button[name="folder-1-0"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0"]);
    await Promise.all(
      checkboxes.map((checkbox) => expect(checkbox).not.toBeHidden()),
    );
    await egressTableWrapper.locator('role=button[name="folder-2-0"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-0"]);
    await Promise.all(
      checkboxes.map((checkbox) => expect(checkbox).not.toBeHidden()),
    );
    await egressTableWrapper.locator('role=button[name="Home"]').click();
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await validateFolderPath(page, ["Home"]);
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    await Promise.all(
      checkboxes.map((checkbox) => expect(checkbox).toBeHidden()),
    );
  });

  test("Should hide checkboxes from the table head, if there are not contents inside the folder, when the egress is the source table", async ({
    page,
  }) => {
    await expect(page.getByTestId("egress-folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(
      page.getByTestId("egress-folder-table-loader"),
    ).not.toBeVisible();
    const egressTableWrapper = page.getByTestId("egress-table-wrapper");
    await validateFolderPath(page, ["Home"]);
    const checkboxes = await egressTableWrapper
      .locator('table input[type="checkbox"]')
      .all();

    await Promise.all(
      checkboxes.map((checkbox) => expect(checkbox).toBeHidden()),
    );
    await egressTableWrapper.locator('role=button[name="folder-1-0"]').click();
    await validateFolderPath(page, ["Home", "folder-1-0"]);
    await egressTableWrapper.locator('role=button[name="folder-2-0"]').click();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-0"]);
    await egressTableWrapper.locator('role=button[name="folder-3-0"]').click();
    await validateFolderPath(page, [
      "Home",
      "folder-1-0",
      "folder-2-0",
      "folder-3-0",
    ]);
    await expect(
      page
        .getByTestId("egress-container")
        .getByText("There are no documents currently in this folder"),
    ).toBeVisible();
  });
});
