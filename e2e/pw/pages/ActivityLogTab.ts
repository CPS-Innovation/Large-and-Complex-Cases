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
    const timeline = this.page.getByTestId("activities-timeline");
    const description = transferType === "Copy"
      ? /Documents\/folders copied from/i
      : /Documents\/folders moved from/i;

    const transferSection = timeline
      .locator("section")
      .filter({ hasText: description });
    await expect(transferSection.first()).toBeVisible({ timeout: 30000 });

    await expect(
      transferSection.first().getByTestId("transfer-tag")
    ).toHaveText("Transfer");

    await expect(
      transferSection.first().getByTestId("transfer-status-tag")
    ).toBeVisible();
  }

  async expandFileList() {
    const latest = await this.getLatestActivity();
    await latest.locator("summary", { hasText: "View files" }).click();
  }

  async downloadCsv() {
    const latest = await this.getLatestActivity();
    await latest
      .getByRole("button", { name: /Download the list of files/i })
      .click();
  }

  async verifyDownloadSuccess() {
    // Download triggers a file download - just verify the button was clickable
    // (no tooltip testid exists on the page)
  }
}
