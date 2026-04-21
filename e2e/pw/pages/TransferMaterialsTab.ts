import { Page } from "@playwright/test";

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
    await this.page.getByLabel("Select folders and files").check();
  }

  async selectEgressFiles(indices: number[]) {
    const checkboxes = await this.page
      .getByTestId("egress-table-wrapper")
      .locator("tbody tr")
      .locator('input[type="checkbox"]')
      .all();
    for (const index of indices) {
      await checkboxes[index].check();
    }
  }

  async selectNetAppFiles(indices: number[]) {
    const checkboxes = await this.page
      .getByTestId("netapp-table-wrapper")
      .locator("tbody tr")
      .locator('input[type="checkbox"]')
      .all();
    for (const index of indices) {
      await checkboxes[index].check();
    }
  }

  async selectAllNetAppFiles() {
    await this.page.getByLabel("Select folders and files").check();
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

    // Assert it was the success banner, not the error page
    if (await errorHeading.isVisible()) {
      const errorText = await this.page.locator("main").innerText();
      throw new Error(`Transfer failed: ${errorText}`);
    }
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
