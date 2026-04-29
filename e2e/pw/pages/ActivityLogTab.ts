import { Page, expect } from "@playwright/test";

export class ActivityLogTab {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async waitForLogs() {
    await this.page
      .getByTestId("activities-timeline")
      .waitFor({ state: "visible", timeout: 30000 });
  }

  async getLatestActivity() {
    return this.page
      .getByTestId("activities-timeline")
      .locator("section")
      .first();
  }

  async verifyTransferLogged(transferType: "Copy" | "Move") {
    const section = await this.getTransferSection(transferType);
    await expect(section).toBeVisible({ timeout: 30000 });

    await expect(
      section.getByTestId("transfer-tag")
    ).toHaveText("Transfer");

    await expect(
      section.getByTestId("transfer-status-tag")
    ).toBeVisible();
  }

  async expandFileList(transferType: "Copy" | "Move" = "Copy") {
    const section = await this.getTransferSection(transferType);
    await section.locator("summary", { hasText: "View files" }).click();
  }

  async downloadCsv(transferType: "Copy" | "Move" = "Copy") {
    const section = await this.getTransferSection(transferType);
    await section
      .getByRole("button", { name: /Download the list of files/i })
      .click();
  }

  private async getTransferSection(transferType: "Copy" | "Move") {
    const timeline = this.page.getByTestId("activities-timeline");
    const description = transferType === "Copy"
      ? /Documents\/folders copied from/i
      : /Documents\/folders moved from/i;

    return timeline
      .locator("section")
      .filter({ hasText: description })
      .first();
  }

  async verifyDownloadSuccess() {
    // ui-spa exposes activity-download-tooltip after download click
    await expect(
      this.page.getByTestId("activity-download-tooltip")
    ).toBeVisible({ timeout: 10000 });
  }
}
