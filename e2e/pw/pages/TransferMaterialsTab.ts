import { expect } from "@playwright/test";
import { TransferMaterialsTabApi } from "./TransferMaterialsTabApi";
import { BaseTransferMaterialsTab } from "./BaseTransferMaterialsTab";

/**
 * Old-screen Transfer Materials page object. Egress-side and common helpers come
 * from `BaseTransferMaterialsTab`; this class carries the old-screen selectors
 * (two side-by-side panels, per-panel inset Copy/Move, confirm modal).
 */
export class TransferMaterialsTab
  extends BaseTransferMaterialsTab
  implements TransferMaterialsTabApi
{
  protected readonly netAppWrapperTestId = "netapp-table-wrapper";

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

  async selectAction(
    action: "Copy" | "Move",
    direction: "egressToNetApp" | "netAppToEgress" = "egressToNetApp",
  ) {
    // Old screen has a Copy/Move control in each panel's inset: the NetApp inset
    // drives Egress → NetApp, the Egress inset drives NetApp → Egress.
    const inset =
      direction === "egressToNetApp"
        ? "netapp-inset-text"
        : "egress-inset-text";
    await this.page
      .getByTestId(inset)
      .getByRole("button", { name: action })
      .click();
  }

  async confirmTransfer(_action: "Copy" | "Move") {
    // The old-screen confirm modal is action-agnostic (the "I want to
    // copy/move" radio matches either), so `_action` is accepted only to
    // satisfy the shared contract.
    const modal = this.page.getByTestId("div-modal");
    await modal.waitFor({ state: "visible", timeout: 30000 });
    await modal.getByLabel(/I want to (copy|move)/).click();
    await modal.getByRole("button", { name: "Continue" }).click();
  }

  async waitForTransferComplete(timeout: number = 300_000) {
    const successBanner = this.page.getByTestId(
      "transfer-success-notification-banner",
    );
    const errorHeading = this.page.locator(
      'text="There is a problem transferring files"',
    );

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
    return await this.page.getByTestId("transfer-progress-metrics").innerText();
  }

  async selectNetAppFileByName(fileName: string) {
    const row = this.page
      .getByTestId("netapp-table-wrapper")
      .locator("tbody tr", { hasText: fileName });
    const checkbox = row.locator('input[type="checkbox"]');
    await checkbox.scrollIntoViewIfNeeded();
    await checkbox.check();
  }

  /**
   * Select a NetApp source row by exact filename; throws a clear fixture-missing
   * message if absent. The panel's unreliable sort + pagination make locating a
   * just-uploaded file flaky, so the default-mode spec runs against a stable
   * pre-seeded fixture (see `tests/seed-netapp-fixture.setup.ts`).
   */
  async selectNetAppFileByExactName(fileName: string) {
    const row = this.page
      .getByTestId("netapp-table-wrapper")
      .locator("tbody tr", { hasText: fileName });
    if ((await row.count()) === 0) {
      throw this.fixtureMissingError(fileName, "NetApp panel");
    }
    const checkbox = row.locator('input[type="checkbox"]').first();
    await checkbox.scrollIntoViewIfNeeded();
    await checkbox.check();
  }

  /**
   * Scroll the NetApp panel until a row containing `fileName` appears, then
   * select it. The listing paginates via continuation tokens, so a
   * freshly-uploaded file at the bottom of the ascending sort isn't in the DOM
   * until scrolling triggers more page loads.
   */
  async selectNetAppFileByNameWithScroll(
    fileName: string,
    timeout: number = 180_000,
  ) {
    const netappTable = this.page.getByTestId("netapp-table-wrapper");
    const fileRow = netappTable.locator("tbody tr", { hasText: fileName });
    const start = Date.now();

    let lastRowCount = -1;
    while (Date.now() - start < timeout) {
      if ((await fileRow.count()) > 0) {
        const checkbox = fileRow.locator('input[type="checkbox"]');
        await checkbox.scrollIntoViewIfNeeded();
        await checkbox.check();
        return;
      }

      const rowCount = await netappTable.locator("tbody tr").count();
      if (rowCount === lastRowCount) {
        // No new rows after the last scroll — pagination either exhausted
        // or no further auto-load was triggered. Wait briefly in case the
        // backend is still indexing the just-uploaded file, then retry.
        await this.page.waitForTimeout(3_000);
      }
      lastRowCount = rowCount;

      // Scroll the last loaded row into view to trigger any infinite-scroll
      // pagination wired to the table container.
      await netappTable
        .locator("tbody tr")
        .last()
        .scrollIntoViewIfNeeded()
        .catch(() => {});
      await this.page.waitForTimeout(1_500);
    }
    throw new Error(
      `Timed out waiting for ${fileName} to be selectable in NetApp panel (timeout: ${timeout}ms, last row count: ${lastRowCount})`,
    );
  }

  /**
   * Both screens navigate to a shared full-page transfer error (no tablist) on a
   * failed or duplicate-rejected transfer. If on it, follow "Back" to case
   * management so the tab can be re-entered. Keyed off the error heading (the
   * old screen detects by text, not route). No-op otherwise.
   */
  async dismissTransferErrorIfPresent(): Promise<void> {
    const errorHeading = this.page.getByRole("heading", {
      name: "There is a problem transferring files",
    });
    if (!(await errorHeading.isVisible().catch(() => false))) return;
    await this.page.getByRole("link", { name: "Back", exact: true }).click();
  }

  /**
   * Verify a file landed in the NetApp panel. Both panels are visible on the old
   * screen, so assert the NetApp table contains the file in place.
   */
  async verifyNetAppContainsFile(
    fileName: string,
    timeout: number = 30_000,
  ): Promise<void> {
    const netappTable = this.page.getByTestId("netapp-table-wrapper");
    await expect(netappTable).toBeVisible({ timeout });
    await expect(netappTable).toContainText(fileName, { timeout });
  }

  /** Wait until at least `expectedCount` file rows appear in the Egress panel folder, refreshing periodically. */
  async waitForFileCount(
    expectedCount: number,
    folderName: string,
    timeout: number = 120_000,
  ) {
    const egressTable = this.page
      .getByTestId("egress-table-wrapper")
      .locator("table");
    const start = Date.now();
    // Check current count first (already navigated into folder)
    let rows = await egressTable.locator("tbody tr").all();
    if (rows.length >= expectedCount) return;

    while (Date.now() - start < timeout) {
      console.log(
        `  Waiting for files: ${rows.length}/${expectedCount} visible, refreshing...`,
      );
      await this.page.waitForTimeout(5_000);
      // Full reload required: Egress file list is fetched once on page load and not auto-refreshed
      await this.page.reload();
      await this.waitForEgressFiles();
      await this.navigateToFolder(folderName);
      await this.waitForEgressFiles();
      rows = await egressTable.locator("tbody tr").all();
      if (rows.length >= expectedCount) return;
    }
    throw new Error(
      `Timed out waiting for ${expectedCount} files (timeout: ${timeout}ms)`,
    );
  }

  // ------------------- Added for Move Test-----------------------------------
  // Confirm a file exists and is of the expected size (in MB)
  async verifyNetAppFileSizeByExactName(
    fileName: string,
    expectedSizeMB: number,
  ): Promise<void> {
    const row = this.page
      .getByTestId("netapp-table-wrapper")
      .locator("tbody tr", { hasText: fileName });

    if ((await row.count()) === 0) {
      throw new Error(
        `A file named '${fileName}' was not found in the NetApp panel.`
      );
    }

    const sizeCell = row.locator("td").nth(1);
    await sizeCell.scrollIntoViewIfNeeded();

    const sizeText = (await sizeCell.textContent())?.trim();

    if (!sizeText) {
      throw new Error(
        `File '${fileName}' was found but its size could not be determined.`
      );
    }

    // Convert to MB
    const match = sizeText.match(/^(\d+(?:\.\d+)?)\s?(KB|MB|GB)$/i);

    if (!match) {
      throw new Error(`Invalid size format for '${fileName}': ${sizeText}`);
    }

    const value = parseFloat(match[1]);
    const unit = match[2].toUpperCase();

    let actualSizeMB: number;

    switch (unit) {
      case "KB":
        actualSizeMB = value / 1000;
        break;
      case "MB":
        actualSizeMB = value;
        break;
      case "GB":
        actualSizeMB = value * 1000;
        break;
      default:
        throw new Error(`Unsupported unit for '${fileName}': ${unit}`);
    }

    // Compare (with tolerance)
    const diff = Math.abs(actualSizeMB - expectedSizeMB);
    const tolerance = expectedSizeMB * 0.01;

    expect(
      diff,
      `File size check for '${fileName}'.\n` +
        `Expected: ${expectedSizeMB.toFixed(2)} MB\n` +
        `Actual:   ${actualSizeMB} MB\n` +
        `Diff:     ${diff.toFixed(2)} MB (should not exceed ${tolerance.toFixed(2)} MB)`
    ).toBeLessThanOrEqual(tolerance);
  }
  // --------------------------------------------------------------------------
}
