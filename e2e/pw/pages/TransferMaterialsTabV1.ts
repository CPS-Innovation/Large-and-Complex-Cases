import { Page, expect } from "@playwright/test";
import { TransferMaterialsTabApi } from "./TransferMaterialsTabApi";
import { TransferDestinationPage } from "./TransferDestinationPage";

/**
 * New-screen (v1) Transfer Materials page object, selected by
 * `getTransferMaterialsTab` when `TRANSFER_MATERIALS_V1` is on. Differs from the
 * old screen: NetApp table renamed "shared drive"; Copy/Move are
 * `Copy selected` / `Move selected` buttons; direction toggles via a
 * `View Shared Drive` / `View Egress` link; no confirm modal (Copy/Move navigate
 * to a destination-tree page, driven by `TransferDestinationPage`); errors use
 * the new `transfer-errors` / `transfer-permissions-error` /
 * `transfer-resolve-file-path` routes. The Egress table is unchanged.
 */
export class TransferMaterialsTabV1 implements TransferMaterialsTabApi {
  private readonly page: Page;
  // Remembered so confirmTransfer knows whether the destination-page button
  // reads "Copy to …" or "Move to …".
  private lastAction: "Copy" | "Move" = "Copy";

  constructor(page: Page) {
    this.page = page;
  }

  /** Click the shared Copy/Move control. Renders in a top and bottom bar (target
   * the first); Move only appears when the source is Egress. */
  private async clickTransferControl(action: "Copy" | "Move"): Promise<void> {
    this.lastAction = action;
    await this.page
      .getByRole("button", { name: `${action} selected` })
      .first()
      .click();
  }

  /**
   * Wait for a source panel's listing to load. The loader mounts a tick after
   * the fetch starts, so wait for it to appear then disappear; a bare
   * wait-for-hidden races and returns against an empty table. Tolerate a rare
   * instant load where the spinner never shows.
   */
  private async waitForFolderSettled(tableName: string): Promise<void> {
    const loader = this.page.getByTestId(`${tableName}-folder-table-loader`);
    await loader.waitFor({ state: "visible", timeout: 10_000 }).catch(() => {});
    await loader.waitFor({ state: "hidden", timeout: 120_000 });
  }

  async waitForEgressFiles(): Promise<void> {
    await this.waitForFolderSettled("egress");
  }

  async waitForNetAppFiles(): Promise<void> {
    await this.waitForFolderSettled("shared drive");
  }

  async switchToNetAppSource(): Promise<void> {
    // Toggle renders in a top and bottom bar; target the first.
    await this.page
      .getByRole("button", { name: "View Shared Drive" })
      .first()
      .click();
  }

  async selectAllEgressFiles(): Promise<void> {
    await this.page
      .getByTestId("egress-table-wrapper")
      .getByLabel("Select folders and files")
      .check();
  }

  async selectNetAppFiles(indices: number[]): Promise<void> {
    // Select per-row checkboxes by aria-label (page-scoped, forced). Only the
    // shared-drive panel shows while it's the source, so these are its rows;
    // a `tbody tr` chain doesn't interact reliably against the real table.
    const checkboxes = this.page.locator(
      'input[type="checkbox"][aria-label^="select file"], input[type="checkbox"][aria-label^="select folder"]',
    );
    for (const index of indices) {
      const checkbox = checkboxes.nth(index);
      await checkbox.scrollIntoViewIfNeeded().catch(() => {});
      await checkbox.check({ force: true });
    }
  }

  /**
   * Sort the shared-drive panel by last-modified date descending (two header
   * clicks). Best-effort: the date sort is unreliable and source identity
   * doesn't matter, so a flaky/slow header is tolerated — proceed unsorted.
   */
  async sortNetAppByDateDescending(): Promise<void> {
    const header = this.page
      .getByTestId("shared drive-table-wrapper")
      .getByRole("button", { name: "Last modified date" });
    try {
      await header.click({ timeout: 20_000 });
      await this.waitForNetAppFiles();
      await header.click({ timeout: 20_000 });
      await this.waitForNetAppFiles();
    } catch {
      console.log("  [sortNetAppByDateDescending] date sort skipped (best-effort)");
    }
  }

  /**
   * Select a shared-drive source row by exact filename; throws a clear
   * fixture-missing message if absent (see `tests/seed-netapp-fixture.setup.ts`).
   */
  async selectNetAppFileByExactName(fileName: string): Promise<void> {
    // Gate on the row checkbox being `attached`, not a `visible` `tbody tr`:
    // shared-drive rows can be in the DOM but not "visible" to Playwright,
    // source switches don't reliably re-trigger the loader, and the listing
    // can take well over 30s — so a visibility gate false-negatives.
    const checkbox = this.page
      .locator(`input[type="checkbox"][aria-label="select file ${fileName}"]`)
      .first();
    try {
      await checkbox.waitFor({ state: "attached", timeout: 120_000 });
    } catch {
      throw new Error(
        `Required NetApp fixture '${fileName}' not found in the shared drive panel.\n` +
          `  Seed it via:\n` +
          `    bash:       RUN_SEED=1 npx playwright test --project=seed-netapp-fixture\n` +
          `    powershell: $env:RUN_SEED=1; npx playwright test --project=seed-netapp-fixture\n` +
          `  See README "Required NetApp fixture" for details.`,
      );
    }
    await checkbox.scrollIntoViewIfNeeded().catch(() => {});
    await checkbox.check({ force: true });
  }

  async selectEgressFileByName(fileName: string): Promise<void> {
    const row = this.page
      .getByTestId("egress-table-wrapper")
      .locator("tbody tr", { hasText: fileName });
    const checkbox = row.locator('input[type="checkbox"]');
    await checkbox.scrollIntoViewIfNeeded();
    await checkbox.check();
  }

  async selectAction(action: "Copy" | "Move"): Promise<void> {
    await this.clickTransferControl(action);
  }

  async selectReverseAction(action: "Copy" | "Move"): Promise<void> {
    await this.clickTransferControl(action);
  }

  /**
   * Confirm the transfer. No modal on the new screen: Copy/Move already
   * navigated to the destination tree — pick the first selectable folder (the
   * connected root) and click the `<Copy|Move> to <folder>` button.
   */
  async confirmTransfer(): Promise<void> {
    const destination = new TransferDestinationPage(this.page);
    await destination.waitForLoaded();
    await destination.selectFirstSelectableFolder();
    await destination.confirm(this.lastAction);
  }

  /**
   * Wait for completion: race the (unchanged) success banner against the new
   * error routes, throwing with the page's error text if a transfer fails.
   */
  async waitForTransferComplete(timeout: number = 300_000): Promise<void> {
    const successBanner = this.page.getByTestId(
      "transfer-success-notification-banner",
    );
    const errorRoute =
      /\/case\/\d+\/case-management\/(transfer-errors|transfer-permissions-error|transfer-resolve-file-path)/;

    await Promise.race([
      successBanner.waitFor({ state: "visible", timeout }),
      this.page.waitForURL(errorRoute, { timeout }),
    ]);

    if (errorRoute.test(this.page.url())) {
      const detail = await this.page
        .locator("main")
        .innerText()
        .catch(() => "");
      throw new Error(
        `Transfer failed — navigated to ${this.page.url()}\n${detail}`,
      );
    }

    await expect(successBanner).toBeVisible();
  }

  /**
   * If parked on a transfer error route (after a failed or duplicate-rejected
   * transfer), follow the page's "Back" link to case management so the tab can
   * be re-entered. No-op otherwise.
   */
  async dismissTransferErrorIfPresent(): Promise<void> {
    const errorRoute =
      /\/case\/\d+\/case-management\/(transfer-errors|transfer-permissions-error|transfer-resolve-file-path)/;
    if (!errorRoute.test(this.page.url())) return;
    await this.page.getByRole("link", { name: "Back", exact: true }).click();
  }

  /**
   * Verify a file landed in the shared drive: switch to it as source and wait
   * for the file's row checkbox to attach (attached-not-visible, as in
   * selectNetAppFileByExactName). Timeout covers slow load + indexing lag.
   */
  async verifyNetAppContainsFile(
    fileName: string,
    timeout: number = 60_000,
  ): Promise<void> {
    await this.switchToNetAppSource();
    await this.waitForNetAppFiles();
    const checkbox = this.page
      .locator(`input[type="checkbox"][aria-label="select file ${fileName}"]`)
      .first();
    await checkbox.waitFor({ state: "attached", timeout });
  }

  async navigateToFolder(folderName: string): Promise<void> {
    await this.page.getByRole("button", { name: folderName }).click();
  }

  /**
   * Wait for a file to appear in the Egress panel. Egress doesn't auto-refresh,
   * so each retry does a full `page.reload()` (busts the React Query cache;
   * in-app re-navigation serves the stale listing) and re-navigates the folder
   * path. The transfer route + Egress-as-source survive the reload.
   */
  async waitForEgressFileByName(
    fileName: string,
    folderPath: string[],
    timeout: number = 300_000,
  ): Promise<void> {
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
      `Timed out waiting for ${fileName} to appear in Egress panel (timeout: ${timeout}ms)`,
    );
  }
}
