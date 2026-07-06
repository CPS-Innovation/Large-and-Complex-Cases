import { type Page, expect } from "@playwright/test";
export class TransferMaterialsSourcePage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await this.page.waitForFunction(
      (expected) => location.pathname === expected,
      url,
      { timeout: 50000 },
    );
  }

  async verifyPageElements() {
    await expect(this.page.locator("h1")).toHaveText("Thunderstruck");
    await expect(this.page.getByTestId("case-urn")).toHaveText("45AA2098221");
    await expect(this.page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
  }

  async verifyTransferLoaderVisible(
    transferSource: "egress" | "shared-drive",
    sameUser: boolean,
    userName: string = "dev_user@example.org",
  ) {
    await expect(
      this.page.getByTestId("transfer-source-wrapper"),
    ).not.toBeVisible();
    await expect(this.page.getByTestId("transfer-spinner")).toBeVisible({
      timeout: 30000,
    });
    if (sameUser) {
      await expect(
        this.page.getByTestId("tab-content-transfer-materials"),
      ).toContainText(
        `${transferSource === "egress" ? "Completing transfer from Egress to Shared Drive..." : "Completing transfer from Shared Drive to Egress..."}`,
      );
    }
    if (!sameUser) {
      await expect(
        this.page.getByTestId("tab-content-transfer-materials"),
      ).toContainText(`${userName} is currently transferring`);
    }
  }

  async verifyTransferLoaderHidden() {
    await expect(this.page.getByTestId("transfer-spinner")).not.toBeVisible({
      timeout: 30000,
    });
  }

  async verifyTransferStats(value: string) {
    await expect(
      this.page.getByTestId("transfer-progress-metrics"),
    ).toBeVisible();
    await expect(
      this.page.getByTestId("transfer-progress-metrics"),
    ).toContainText(value);
  }

  async verifyTransferStatsHidden() {
    await expect(
      this.page.getByTestId("transfer-progress-metrics"),
    ).not.toBeVisible();
  }

  async verifyTransferSourceHidden() {
    await expect(
      this.page.getByTestId("transfer-source-wrapper"),
    ).not.toBeVisible();
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
    await expect(
      this.page.getByTestId("transfer-source-wrapper"),
    ).toBeVisible();
    await expect(
      this.page.getByTestId("transfer-source-wrapper").locator("h2"),
    ).toHaveText("Transfer from Egress to the Shared Drive");
    await expect(
      this.page.getByTestId("transfer-source-description"),
    ).toHaveText(
      "Select the files or folders you want to transfer. Then choose where to save them on the Shared Drive.",
    );
    await expect(
      this.page.getByTestId("toggle-transfer-direction").first(),
    ).toHaveText("View Shared Drive");
    await expect(this.page.getByTestId("transfer-controls")).toHaveCount(2);
    await expect(
      this.page.getByRole("button", { name: "Disconnect Shared Drive" }),
    ).toBeVisible();
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
    await expect(
      this.page.getByTestId("toggle-transfer-direction").first(),
    ).toHaveText("View Egress");
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

  async verifyCheckboxesVisibility(visible: boolean, count: number) {
    const checkboxes = await this.page
      .locator('table input[type="checkbox"]')
      .all();
    expect(checkboxes.length).toBe(count);
    if (visible) {
      await Promise.all(
        checkboxes.map((checkbox) =>
          expect(checkbox).toBeVisible({ timeout: 30000 }),
        ),
      );
    } else {
      await Promise.all(
        checkboxes.map((checkbox) =>
          expect(checkbox).toBeHidden({ timeout: 30000 }),
        ),
      );
    }
  }

  async clickToggleTransferDirection() {
    await this.page.getByTestId("toggle-transfer-direction").first().click();
  }

  async toggleCheckbox(
    index: number,
    sourceType: "egress" | "shared-drive" = "egress",
  ) {
    const checkboxes = this.page
      .getByTestId(`${sourceType}-table-wrapper`)
      .locator('input[type="checkbox"]');
    await checkboxes.nth(index).click();
  }

  async verifyCopyBtnEnabled(enabled: boolean) {
    const copyBtns = this.page.getByRole("button", { name: "Copy selected" });
    await expect(copyBtns).toHaveCount(2);
    if (enabled) {
      await expect(copyBtns.nth(0)).toBeEnabled();
      await expect(copyBtns.nth(1)).toBeEnabled();
    } else {
      await expect(copyBtns.nth(0)).toBeDisabled();
      await expect(copyBtns.nth(1)).toBeDisabled();
    }
  }
  async verifyMoveBtnEnabled(enabled: boolean) {
    const moveBtns = this.page.getByRole("button", { name: "Move selected" });
    await expect(moveBtns).toHaveCount(2);
    if (enabled) {
      await expect(moveBtns.nth(0)).toBeEnabled();
      await expect(moveBtns.nth(1)).toBeEnabled();
    } else {
      await expect(moveBtns.nth(0)).toBeDisabled();
      await expect(moveBtns.nth(1)).toBeDisabled();
    }
  }

  async verifyMoveBtnHidden() {
    await expect(
      this.page.getByRole("button", { name: "Move selected" }),
    ).toHaveCount(0);
  }

  async clickCopyBtn() {
    const copyBtns = this.page.getByRole("button", { name: "Copy selected" });
    await copyBtns.nth(0).click();
  }

  async clickMoveBtn() {
    const moveBtns = this.page.getByRole("button", { name: "Move selected" });
    await moveBtns.nth(0).click();
  }

  async validateTransferSuccessBanner(
    relativePaths: { folderPath: string; files: string[] }[],
    transferType: "copy" | "move",
  ) {
    const successBanner = this.page.getByTestId(
      "transfer-success-notification-banner",
    );
    await expect(successBanner).toBeVisible();

    await expect(successBanner.locator("h2")).toHaveText("Success");
    await expect(successBanner.locator("b")).toHaveText(
      "The materials have been transferred.",
    );
    await expect(
      successBanner.getByTestId("transfer-success-destination-folder"),
    ).toBeVisible();
    await expect(
      successBanner.getByTestId("transfer-success-destination-folder"),
    ).toHaveText("folder-2-0");

    await expect(successBanner.locator("details>summary")).toHaveText(
      `Show ${transferType === "copy" ? "copied" : "moved"} materials`,
    );
    await expect(successBanner.getByTestId("transfer-files")).not.toBeVisible();
    await successBanner.locator("details>summary").click();
    await expect(successBanner.getByTestId("transfer-files")).toBeVisible();
    const activityFileSections = successBanner
      .getByTestId("transfer-files")
      .locator("section");

    await expect(activityFileSections).toHaveCount(relativePaths.length);
    await Promise.all(
      relativePaths.map(async (relativePath, index) => {
        await expect(
          activityFileSections.nth(index).getByTestId("transfer-relative-path"),
        ).toHaveText(relativePath.folderPath);
        await expect(
          activityFileSections.nth(index).locator("ul").locator("li"),
        ).toHaveCount(relativePath.files.length);
        const fileItems = activityFileSections
          .nth(index)
          .locator("ul")
          .locator("li");
        relativePath.files.forEach(async (file, i) => {
          await expect(fileItems.nth(i)).toHaveText(file);
        });
      }),
    );
    await successBanner.locator("details>summary").click();
    await expect(successBanner.getByTestId("transfer-files")).not.toBeVisible({
      timeout: 50000,
    });
  }

  async validateTransferSuccessBannerHidden() {
    const successBanner = this.page.getByTestId(
      "transfer-success-notification-banner",
    );
    await expect(successBanner).not.toBeVisible();
  }
}
