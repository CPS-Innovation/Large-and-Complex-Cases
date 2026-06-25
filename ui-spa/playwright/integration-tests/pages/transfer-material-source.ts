import { type Page, expect } from "@playwright/test";

export class TransferMaterialsSourcePage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  async verifyPageElements() {
    await expect(this.page.locator("h1")).toHaveText("Thunderstruck");
    await expect(this.page.getByTestId("case-urn")).toHaveText("45AA2098221");
    await expect(
      this.page.getByRole("button", { name: "Disconnect Shared Drive" }),
    ).toBeVisible();
    await expect(this.page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
  }

  async verifyTransferSourceTableLoader(
    transferSource: "egress" | "shared-drive",
    visible: boolean,
  ) {
    if (visible) {
      await expect(this.page.getByTestId("folder-table-loader")).toBeVisible({
        timeout: 30000,
      });
      if (transferSource === "egress") {
        await expect(this.page.getByTestId("folder-table-loader")).toHaveText(
          "Loading folders from Egress",
        );
      } else {
        await expect(this.page.getByTestId("folder-table-loader")).toHaveText(
          "Loading folders from Shared Drive",
        );
      }
      return;
    }
    await expect(this.page.getByTestId("folder-table-loader")).not.toBeVisible({
      timeout: 30000,
    });
  }

  async verifyEgressTransferSourceElements() {
    await expect(this.page.locator("h2")).toHaveText(
      "Transfer from Egress to the Shared Drive",
    );
    await expect(
      this.page.getByTestId("transfer-source-description"),
    ).toHaveText(
      "Select the files or folders you want to transfer. Then choose where to save them on the Shared Drive.",
    );
    await expect(
      this.page.getByTestId("toggle-transfer-direction").first(),
    ).toHaveText("View Shared Drive");
    await expect(this.page.getByTestId("transfer-controls")).toHaveCount(2);
  }

  async verifySharedDriveTransferSourceElements() {
    await expect(this.page.locator("h2")).toHaveText(
      "Transfer from Shared Drive to Egress",
    );
    await expect(
      this.page.getByTestId("transfer-source-description"),
    ).toHaveText(
      "Select the files or folders you want to transfer. Then choose where to save them on Egress.",
    );
    await expect(this.page.getByTestId("toggle-transfer-direction")).toHaveText(
      "View Egress",
    );
    await expect(this.page.getByTestId("transfer-controls")).toHaveCount(2);
  }

  async verifyFolderPath(expectedFolders: string[]) {
    const texts = await this.page
      .getByTestId("folder-path")
      .locator("ol>li")
      .allTextContents();
    expect(texts).toEqual(expectedFolders);
  }

  async validateTableColumnHeaders(expectedHeaders?: string[]) {
    const expectedValues = expectedHeaders ?? [
      "",
      " Folder/file name",
      " Last modified date",
      " Size",
    ];
    const tableHeadValues = await this.page
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual(expectedValues);
  }

  async validateTableRowValues(expectedRowValues: string[][]) {
    const getRow = async (rowIndex: number) =>
      this.page
        .locator(`table tbody tr:nth-child(${rowIndex}) td`)
        .allTextContents();

    for (let i = 0; i < expectedRowValues.length; i++) {
      const actual = await getRow(i + 1);
      expect(actual).toEqual(expectedRowValues[i]);
    }
  }

  async handleFolderClick(folderName: string) {
    await this.page.getByRole("button", { name: folderName }).click();
  }

  async verifyNoResults() {
    await expect(this.page.getByTestId("no-documents-text")).toBeVisible();
    await expect(this.page.getByTestId("no-documents-text")).toHaveText(
      "There are no documents currently in this folder",
    );
  }
}
