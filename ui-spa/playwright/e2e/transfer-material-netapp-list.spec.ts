import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";
import { Page } from "@playwright/test";

test.describe("transfer material netapp list", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
  });
  const validateFolderPath = async (page: Page, expectedResult: string[]) => {
    const texts = await page
      .getByTestId("netapp-table-wrapper")
      .locator("ol>li")
      .allTextContents();
    expect(texts).toEqual(expectedResult);
  };

  test("Should show the netapp folders results correctly when Netapp is the source table", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer between Egress and Shared Drive");
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await page
      .getByRole("button", { name: "from the Shared Drive to Egress" })
      .click();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer between Shared Drive and Egress");
    const netappTableWrapper = page.getByTestId("netapp-table-wrapper");
    await validateFolderPath(page, ["netapp"]);
    const tableHeadValues = await netappTableWrapper
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual([
      "",
      " Folder/file name",
      " Last modified date",
      " Size",
    ]);
    const row1Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual(["", "folder-1-0", "--", "--"]);
    const row2Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual(["", "folder-1-1", "--", "--"]);

    const row3Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(3) td")
      .allTextContents();
    expect(row3Values).toEqual(["", "file-1-0.pdf", "02/01/2000", "1.23 KB"]);

    const row4Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(4) td")
      .allTextContents();
    expect(row4Values).toEqual(["", "file-1-1.pdf", "03/01/2000", "2.26 MB"]);
  });

  test("Should show the netapp folders results correctly when Netapp is the destination table", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer between Egress and Shared Drive");
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    const netappTableWrapper = page.getByTestId("netapp-table-wrapper");
    await validateFolderPath(page, ["netapp"]);
    const tableHeadValues = await netappTableWrapper
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual([" Folder/file name", " Size"]);
    const row1Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual(["folder-1-0", "--"]);
    const row2Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual(["folder-1-1", "--"]);

    const row3Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(3) td")
      .allTextContents();
    expect(row3Values).toEqual(["file-1-0.pdf", "1.23 KB"]);

    const row4Values = await netappTableWrapper
      .locator("table tbody tr:nth-child(4) td")
      .allTextContents();
    expect(row4Values).toEqual(["file-1-1.pdf", "2.26 MB"]);
  });

  test("Should correctly navigate through the netapp folders", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer between Egress and Shared Drive");
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();

    const netappTableWrapper = page.getByTestId("netapp-table-wrapper");
    await validateFolderPath(page, ["netapp"]);
    await netappTableWrapper.locator('role=button[name="folder-1-0"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["netapp", "folder-1-0"]);
    await netappTableWrapper.locator('role=button[name="folder-2-0"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["netapp", "folder-1-0", "folder-2-0"]);
    await netappTableWrapper.locator('role=button[name="folder-3-0"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await netappTableWrapper.locator('role=button[name="folder-1-0"]').click();
    await validateFolderPath(page, ["netapp", "folder-1-0"]);
    await netappTableWrapper.locator('role=button[name="folder-2-1"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["netapp", "folder-1-0", "folder-2-1"]);
    await netappTableWrapper.locator('role=button[name="folder-3-1"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, [
      "netapp",
      "folder-1-0",
      "folder-2-1",
      "folder-3-1",
    ]);
    await netappTableWrapper.locator('role=button[name="folder-2-1"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["netapp", "folder-1-0", "folder-2-1"]);
    await expect(page.locator('role=button[name="folder-3-0"]')).toBeVisible();
    await expect(page.locator('role=button[name="folder-3-1"]')).toBeVisible();
    await netappTableWrapper.locator('role=button[name="netapp"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await validateFolderPath(page, ["netapp"]);
    await expect(page.locator('role=button[name="folder-1-0"]')).toBeVisible();
    await expect(page.locator('role=button[name="folder-1-1"]')).toBeVisible();
  });

  test("Should show no results page if the netapp folders return empty results", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/netapp/files", async () => {
        await delay(500);
        return HttpResponse.json({
          data: { fileData: [], folderData: [] },
          pagination: {
            nextContinuationToken: null,
          },
        });
      }),
    );
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer between Egress and Shared Drive");
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Shared drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["netapp"]);

    await expect(
      page.getByTestId("netapp-container").getByTestId("no-documents-text"),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-container").getByTestId("no-documents-text"),
    ).toHaveText("There are no documents currently in this folder");
  });
});
