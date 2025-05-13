import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";

test.describe("transfer material egress list", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
  });
  const validateFolderPath = async (page, expectedResult: string[]) => {
    const texts = await page.locator("ol>li").allTextContents();
    expect(texts).toEqual(expectedResult);
  };

  test("Should show the transfer material tab with correct initial content", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByText("45AA2098221")).toBeVisible();
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText(
      "Transfer folders and files between egress and the shared drive",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText(
      "Select the folders and files you want to transfer, then choose a destination. You can switch the source and destination if needed.",
    );

    await expect(page.getByTestId("egress-container")).toBeVisible();
    await expect(page.getByTestId("netapp-container")).toBeVisible();
    await expect(
      page
        .getByTestId("egress-container")
        .locator("h3", { hasText: "Egress Inbound documents" }),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("netapp-container")
        .locator("h3", { hasText: "Shared drive" }),
    ).toBeVisible();
  });

  test("Should show the egress folders results correctly", async ({ page }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText(
      "Transfer folders and files between egress and the shared drive",
    );
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, ["Home"]);
    const tableHeadValues = await page
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual([
      "",
      " Folder/file name",
      " Last modified date",
      " Size",
    ]);
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual(["", "folder-1-0", "02/01/2000", ""]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual(["", "folder-1-1", "03/01/2000", ""]);

    const row3Values = await page
      .locator("table tbody tr:nth-child(3) td")
      .allTextContents();
    expect(row3Values).toEqual(["", "file-1-2.pdf", "03/01/2000", "1.23 KB"]);
  });

  test("Should correctly navigate through the egress folders", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText(
      "Transfer folders and files between egress and the shared drive",
    );
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, ["Home"]);
    await page.locator('role=button[name="folder-1-0"]').click();
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0"]);
    await page.locator('role=button[name="folder-2-0"]').click();
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-0"]);
    await page.locator('role=button[name="folder-3-0"]').click();
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await page.locator('role=button[name="folder-1-0"]').click();
    await validateFolderPath(page, ["Home", "folder-1-0"]);
    await page.locator('role=button[name="folder-2-1"]').click();
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-1"]);
    await page.locator('role=button[name="folder-3-1"]').click();
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, [
      "Home",
      "folder-1-0",
      "folder-2-1",
      "folder-3-1",
    ]);
    await page.locator('role=button[name="folder-2-1"]').click();
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, ["Home", "folder-1-0", "folder-2-1"]);
    await expect(page.locator('role=button[name="folder-3-0"]')).toBeVisible();
    await expect(page.locator('role=button[name="folder-3-1"]')).toBeVisible();
    await page.locator('role=button[name="Home"]').click();
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
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
        "https://mocked-out-api/api/egress/workspaces/egress_1/files",
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
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText(
      "Transfer folders and files between egress and the shared drive",
    );
    await expect(page.getByTestId("folder-table-loader")).toBeVisible();
    await expect(page.getByText(`Loading folders from Egress`)).toBeVisible();
    await expect(page.getByTestId("folder-table-loader")).not.toBeVisible();
    await validateFolderPath(page, ["Home"]);
    await expect(
      page
        .getByTestId("egress-container")
        .getByText("There are no documents currently in this folder"),
    ).toBeVisible();
  });
});
