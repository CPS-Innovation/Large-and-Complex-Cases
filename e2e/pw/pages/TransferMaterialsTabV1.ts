import { Page, expect } from "@playwright/test";
import { TransferMaterialsTabApi } from "./TransferMaterialsTabApi";
import { TransferDestinationPage } from "./TransferDestinationPage";

/**
 * New-screen (redesigned / v1) Transfer Materials page object.
 *
 * Selected by `getTransferMaterialsTab` when `TRANSFER_MATERIALS_V1` is on.
 * The old-screen page object (`TransferMaterialsTab`) is unchanged; this class
 * carries the new-screen selectors and flow:
 *   - the NetApp table is now named "shared drive" (wrapper id
 *     `shared drive-table-wrapper`, loader `shared drive-folder-table-loader`);
 *   - Copy/Move are `Copy selected` / `Move selected` buttons in
 *     TransferControls, not per-panel inset text;
 *   - the direction toggle is a `View Shared Drive` / `View Egress` link button;
 *   - there is no confirm modal — `Copy selected` / `Move selected` navigate to
 *     a destination-tree page where you pick a folder and confirm with
 *     `<Copy|Move> to <folder>` (driven via `TransferDestinationPage`);
 *   - success/error use the unchanged success banner + the new error routes
 *     (`transfer-errors` / `transfer-permissions-error` / `transfer-resolve-file-path`).
 * The Egress table keeps its name, per-row checkboxes and select-all label.
 */
export class TransferMaterialsTabV1 implements TransferMaterialsTabApi {
  private readonly page: Page;
  // Remembered from selectAction/selectReverseAction so confirmTransfer knows
  // whether the destination-page confirm button reads "Copy to …" or "Move to …".
  private lastAction: "Copy" | "Move" = "Copy";

  constructor(page: Page) {
    this.page = page;
  }

  /** Click the shared Copy/Move control in TransferControls. On the new screen
   * both transfer directions use the same `Copy selected` / `Move selected`
   * buttons (Move only renders when the source is Egress). The controls render
   * in both a top and bottom bar, so target the first. */
  private async clickTransferControl(action: "Copy" | "Move"): Promise<void> {
    this.lastAction = action;
    await this.page
      .getByRole("button", { name: `${action} selected` })
      .first()
      .click();
  }

  /**
   * Wait until a source panel's folder listing has finished loading. The loader
   * spinner mounts a tick *after* navigation kicks off the fetch, so a bare
   * wait-for-hidden races: Playwright treats the not-yet-mounted spinner as
   * "hidden" and returns instantly, before the list has actually loaded — and a
   * subsequent instantaneous `.count()` check then sees an empty table. Wait for
   * the spinner to appear first, then disappear; tolerate a rare instant load
   * where it never shows.
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
    // The toggle renders in both a top and bottom control bar; target the first.
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
    // Target the per-row checkboxes by their aria-label ("select file …" /
    // "select folder …", set by the Checkbox component), page-scoped. On the
    // new screen only the shared-drive panel is shown while it's the source, so
    // these are its rows. This avoids a `tbody tr` chain that doesn't interact
    // reliably against the real shared-drive table.
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
   * clicks). Best-effort: the panel's date sort is unreliable and source
   * identity doesn't matter for the transfer, so a flaky/slow header (e.g. a
   * folder churning through many subfolders) is tolerated — proceed unsorted.
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
   * Select a shared-drive (NetApp) source row by exact filename. Throws with a
   * clear fixture-missing message if the row isn't in the loaded listing — the
   * default-mode NetApp -> Egress spec runs against a stable pre-seeded fixture
   * (see `helpers/constants.ts` NETAPP_FIXTURE_FILENAME and
   * `tests/seed-netapp-fixture.setup.ts`).
   */
  async selectNetAppFileByExactName(fileName: string): Promise<void> {
    // Gate on the target row's checkbox by its exact aria-label
    // ("select file <name>"), page-scoped — this is the same element the
    // selection uses, so if it attaches we can select it. We deliberately
    // wait for `attached` rather than a `visible` `tbody tr`: on the real
    // shared-drive table rows can be present in the DOM yet not "visible"
    // to Playwright, and switching source doesn't reliably re-trigger the
    // loader spinner, so a wrapper/`tbody tr` visibility gate spuriously
    // reports a present fixture as missing. The busy environment's listing
    // can also take well over 30s to populate, hence the generous timeout.
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
   * Confirm the transfer. On the new screen there is no modal: `Copy selected` /
   * `Move selected` (via selectAction/selectReverseAction) navigated to the
   * destination page. Pick the target folder in the tree — the connected root
   * folder for Egress→NetApp, or the first Egress subfolder for NetApp→Egress —
   * then click the `<Copy|Move> to <folder>` confirm button.
   */
  async confirmTransfer(): Promise<void> {
    const destination = new TransferDestinationPage(this.page);
    await destination.waitForLoaded();
    await destination.selectFirstSelectableFolder();
    await destination.confirm(this.lastAction);
  }

  /**
   * Wait for the transfer to finish. After confirming, the screen shows the
   * validating/indexing spinner, then returns to case management and shows the
   * success banner (`transfer-success-notification-banner`, unchanged). A failed
   * transfer instead navigates to one of the new error routes.
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
   * If the new screen is parked on a transfer error route (reached after a
   * failed or duplicate-file-rejected transfer), follow the page's "Back"
   * link to return to case management, where the tablist is available again
   * for re-entering the Transfer Materials tab. No-op when not on an error
   * route (e.g. after a successful transfer, which lands on case management
   * with the success banner already).
   */
  async dismissTransferErrorIfPresent(): Promise<void> {
    const errorRoute =
      /\/case\/\d+\/case-management\/(transfer-errors|transfer-permissions-error|transfer-resolve-file-path)/;
    if (!errorRoute.test(this.page.url())) return;
    await this.page.getByRole("link", { name: "Back", exact: true }).click();
  }

  /**
   * Verify a file landed in the shared-drive panel. The new screen shows
   * only the source panel, so switch to the shared drive to view what
   * landed there, then wait for the file's row checkbox (by exact
   * aria-label) to attach — the same attached-not-visible gate
   * `selectNetAppFileByExactName` uses, since shared-drive rows can be in
   * the DOM without being "visible" to Playwright. The generous timeout
   * absorbs both the slow listing load and any post-copy indexing lag.
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
   * Wait until a file with the given name appears in the Egress panel. Egress
   * fetches a folder's list when you navigate into it and doesn't auto-refresh,
   * so each retry does a full `page.reload()` (which busts the React Query cache
   * so the freshly-indexed file is actually re-fetched — an in-app re-navigation
   * serves the stale cached listing) and re-navigates the folder path from the
   * top. On the new screen the transfer-materials route + Egress-as-source
   * survive the reload, so the panel comes back and shows the updated listing.
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
