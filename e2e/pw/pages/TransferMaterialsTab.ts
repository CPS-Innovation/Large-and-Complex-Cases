import { Page, expect } from "@playwright/test";

export class TransferMaterialsTab {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async waitForEgressFiles() {
    await this.page
      .getByTestId("egress-folder-table-loader")
      .waitFor({ state: "hidden" });
  }

  async waitForNetAppFiles() {
    await this.page
      .getByTestId("netapp-folder-table-loader")
      .waitFor({ state: "hidden" });
  }

  async switchToNetAppSource() {
    await this.page
      .getByRole("button", { name: /from the Shared Drive to Egress/i })
      .click();
  }

  async selectAllEgressFiles() {
    await this.page
      .getByTestId("egress-table-wrapper")
      .getByLabel("Select folders and files")
      .check();
  }

  async selectEgressFiles(indices: number[]) {
    const rows = this.page
      .getByTestId("egress-table-wrapper")
      .locator("tbody tr");
    for (const index of indices) {
      await rows.nth(index).locator('input[type="checkbox"]').check();
    }
  }

  async selectNetAppFiles(indices: number[]) {
    const rows = this.page
      .getByTestId("netapp-table-wrapper")
      .locator("tbody tr");
    for (const index of indices) {
      await rows.nth(index).locator('input[type="checkbox"]').check();
    }
  }

  async selectAllNetAppFiles() {
    await this.page
      .getByTestId("netapp-table-wrapper")
      .getByLabel("Select folders and files")
      .check();
  }

  async selectAction(action: "Copy" | "Move") {
    await this.page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: action })
      .click();
  }

  async selectReverseAction(action: "Copy" | "Move") {
    await this.page
      .getByTestId("egress-inset-text")
      .getByRole("button", { name: action })
      .click();
  }

  async confirmTransfer() {
    const modal = this.page.getByTestId("div-modal");
    await modal.waitFor({ state: "visible", timeout: 30000 });
    await modal.getByLabel(/I want to (copy|move)/).click();
    await modal.getByRole("button", { name: "Continue" }).click();
  }

  async waitForTransferComplete(timeout: number = 300_000) {
    const successBanner = this.page.getByTestId("transfer-success-notification-banner");
    const errorHeading = this.page.locator('text="There is a problem transferring files"');

    // Wait for either success banner or error page to appear
    await Promise.race([
      successBanner.waitFor({ state: "visible", timeout }),
      errorHeading.waitFor({ state: "visible", timeout }),
    ]);

    // Fail fast with the on-page error text if the error page won the race
    if (await errorHeading.isVisible()) {
      const errorText = await this.page.locator("main").innerText();
      throw new Error(`Transfer failed: ${errorText}`);
    }

    // Assert the success banner is actually the thing that resolved the race
    await expect(successBanner).toBeVisible();
  }

  async getTransferProgress(): Promise<string> {
    return await this.page
      .getByTestId("transfer-progress-metrics")
      .innerText();
  }

  async selectEgressFileByName(fileName: string) {
    const row = this.page
      .getByTestId("egress-table-wrapper")
      .locator("tbody tr", { hasText: fileName });
    const checkbox = row.locator('input[type="checkbox"]');
    await checkbox.scrollIntoViewIfNeeded();
    await checkbox.check();
  }

  async selectNetAppFileByName(fileName: string) {
    const row = this.page
      .getByTestId("netapp-table-wrapper")
      .locator("tbody tr", { hasText: fileName });
    const checkbox = row.locator('input[type="checkbox"]');
    await checkbox.scrollIntoViewIfNeeded();
    await checkbox.check();
  }

  async navigateToFolder(folderName: string) {
    await this.page.getByRole("button", { name: folderName }).click();
  }

  /**
   * Wait until a file with the given name appears in the Egress panel,
   * reloading and re-navigating into the supplied folder path on each
   * retry. Egress fetches the file list once on page load and does not
   * auto-refresh, so a plain `waitFor` will spin against a stale DOM if
   * the upload is still being indexed when navigation lands.
   *
   * `folderPath` is the sequence of folder names to click into after the
   * page reload (e.g. ["4. Served Evidence", uploadSubfolder]).
   */
  async waitForEgressFileByName(
    fileName: string,
    folderPath: string[],
    timeout: number = 300_000
  ) {
    const egressTable = this.page.getByTestId("egress-table-wrapper");
    const start = Date.now();

    const fileVisible = async () =>
      (await egressTable
        .locator("tbody tr", { hasText: fileName })
        .count()) > 0;

    if (await fileVisible()) return;

    while (Date.now() - start < timeout) {
      console.log(`  Waiting for ${fileName} to be indexed; refreshing...`);
      await this.page.waitForTimeout(5_000);
      await this.page.reload();
      await this.waitForEgressFiles();
      for (const folder of folderPath) {
        await this.navigateToFolder(folder);
        await this.waitForEgressFiles();
      }
      if (await fileVisible()) return;
    }
    throw new Error(
      `Timed out waiting for ${fileName} to appear in Egress panel (timeout: ${timeout}ms)`
    );
  }

  /** Wait until at least `expectedCount` file rows appear in the Egress panel folder, refreshing periodically. */
  async waitForFileCount(expectedCount: number, folderName: string, timeout: number = 120_000) {
    const egressTable = this.page.getByTestId("egress-table-wrapper").locator("table");
    const start = Date.now();
    // Check current count first (already navigated into folder)
    let rows = await egressTable.locator("tbody tr").all();
    if (rows.length >= expectedCount) return;

    while (Date.now() - start < timeout) {
      console.log(`  Waiting for files: ${rows.length}/${expectedCount} visible, refreshing...`);
      await this.page.waitForTimeout(5_000);
      // Full reload required: Egress file list is fetched once on page load and not auto-refreshed
      await this.page.reload();
      await this.waitForEgressFiles();
      await this.navigateToFolder(folderName);
      await this.waitForEgressFiles();
      rows = await egressTable.locator("tbody tr").all();
      if (rows.length >= expectedCount) return;
    }
    throw new Error(`Timed out waiting for ${expectedCount} files (timeout: ${timeout}ms)`);
  }
}
