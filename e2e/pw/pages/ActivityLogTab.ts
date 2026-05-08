import { Page, Locator, expect } from "@playwright/test";

export class ActivityLogTab {
  private readonly page: Page;
  // Section captured by the most recent verifyTransferLogged call. Reused by
  // expandFileList / downloadCsv so they target the current run's entry
  // rather than the first matching section in the timeline (default-mode
  // tests reuse the same case so older Copy/Move entries can remain).
  private currentSection?: Locator;

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

  /**
   * Verify the current run's transfer entry is in the timeline.
   *
   * `uniqueText` must be a string that appears only in the current test's
   * activity entry — typically `uploadSubfolder` or the generated file name.
   * Required because default-mode tests reuse the same case across runs, so
   * matching only on transfer type can pass against an older successful
   * Copy/Move entry that the current run did not create.
   *
   * After this call returns the matched section is cached on the instance,
   * so `expandFileList` / `downloadCsv` operate on the same entry without
   * needing the identifier passed again.
   */
  async verifyTransferLogged(
    transferType: "Copy" | "Move",
    uniqueText: string
  ) {
    if (!uniqueText) {
      throw new Error(
        "verifyTransferLogged requires a uniqueText that identifies the current run (e.g. uploadSubfolder or fileName); see ActivityLogTab.ts"
      );
    }
    const section = this.getTransferSection(transferType, uniqueText);
    await expect(section).toBeVisible({ timeout: 30000 });

    await expect(
      section.getByTestId("transfer-tag")
    ).toHaveText("Transfer");

    await expect(
      section.getByTestId("transfer-status-tag")
    ).toBeVisible();

    this.currentSection = section;
  }

  async expandFileList() {
    const section = this.requireCurrentSection("expandFileList");
    await section.locator("summary", { hasText: "View files" }).click();
  }

  async downloadCsv() {
    const section = this.requireCurrentSection("downloadCsv");
    await section
      .getByRole("button", { name: /Download the list of files/i })
      .click();
  }

  private getTransferSection(
    transferType: "Copy" | "Move",
    uniqueText: string
  ): Locator {
    const timeline = this.page.getByTestId("activities-timeline");
    const description = transferType === "Copy"
      ? /Documents\/folders copied from/i
      : /Documents\/folders moved from/i;

    return timeline
      .locator("section")
      .filter({ hasText: description })
      .filter({ hasText: uniqueText })
      .first();
  }

  private requireCurrentSection(method: string): Locator {
    if (!this.currentSection) {
      throw new Error(
        `${method}() must be called after verifyTransferLogged() so the current run's section is known`
      );
    }
    return this.currentSection;
  }

  async verifyDownloadSuccess() {
    // ui-spa exposes activity-download-tooltip after download click
    await expect(
      this.page.getByTestId("activity-download-tooltip")
    ).toBeVisible({ timeout: 10000 });
  }
}
