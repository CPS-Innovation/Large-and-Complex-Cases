import { expect, Locator } from "@playwright/test";
import { TransferMaterialsTabApi } from "./TransferMaterialsTabApi";
import { TransferDestinationPage } from "./TransferDestinationPage";
import { BaseTransferMaterialsTab } from "./BaseTransferMaterialsTab";

// The new screen navigates to one of these routes on a failed or
// duplicate-rejected transfer (used to detect + recover from the error page).
const TRANSFER_ERROR_ROUTE =
  /\/case\/\d+\/case-management\/(transfer-errors|transfer-permissions-error|transfer-resolve-file-path)/;

/**
 * New-screen (v1) Transfer Materials page object, selected by
 * `getTransferMaterialsTab` when `TRANSFER_MATERIALS_V1` is on. Differs from the
 * old screen: NetApp table renamed "shared drive"; Copy/Move are
 * `Copy selected` / `Move selected` buttons; direction toggles via a
 * `View Shared Drive` / `View Egress` link; no confirm modal (Copy/Move navigate
 * to a destination-tree page, driven by `TransferDestinationPage`); errors use
 * the routes above. Egress-side helpers come from `BaseTransferMaterialsTab`.
 */
export class TransferMaterialsTabV1
  extends BaseTransferMaterialsTab
  implements TransferMaterialsTabApi
{
  protected readonly netAppWrapperTestId = "shared drive-table-wrapper";

  /** Row checkbox for a shared-drive file, by its exact aria-label. */
  private fileCheckbox(fileName: string): Locator {
    return this.page
      .locator(`input[type="checkbox"][aria-label="select file ${fileName}"]`)
      .first();
  }

  // Two V1 layouts exist during the rollout. The older build (currently on
  // staging) shows a "<action> selected" button in the source bar that advances
  // to a separate destination-tree page. The newer build (currently on dev)
  // shows source and destination together with an inline "<action> to <folder>"
  // button and no "selected" step. selectAction/confirmTransfer detect which is
  // present so the specs work against either.

  /** Source-bar control on the older layout ("Copy selected" / "Move selected").
   * Renders in a top and bottom bar; target the first. */
  private selectedControl(action: "Copy" | "Move"): Locator {
    return this.page
      .getByRole("button", { name: `${action} selected` })
      .first();
  }

  /** Inline confirm control ("<action> to <folder>"), used by the newer layout
   * directly and by the older layout's destination-tree page after a folder is
   * picked. */
  private inlineTransferControl(action: "Copy" | "Move"): Locator {
    return this.page
      .getByRole("button", { name: new RegExp(`^${action} to `) })
      .first();
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
    // The direction toggle is a single button whose label flips: "View Shared
    // Drive" while Egress is the source, "View Egress" once the shared drive
    // is. It renders in a top and bottom bar, so target the first of each.
    // Wait for the toggle to render (either label), then only click when Egress
    // is still the source — clicking is a no-op (and a missing-button timeout)
    // if the shared drive is already showing.
    const viewShared = this.page
      .getByRole("button", { name: "View Shared Drive" })
      .first();
    const viewEgress = this.page
      .getByRole("button", { name: "View Egress" })
      .first();
    await Promise.race([
      viewShared.waitFor({ state: "visible", timeout: 30_000 }),
      viewEgress.waitFor({ state: "visible", timeout: 30_000 }),
    ]).catch(() => {});
    if (await viewShared.isVisible().catch(() => false)) {
      await viewShared.click();
    }
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
   * Select a shared-drive source row by exact filename; throws a clear
   * fixture-missing message if absent (see `tests/seed-netapp-fixture.setup.ts`).
   */
  async selectNetAppFileByExactName(fileName: string): Promise<void> {
    // Gate on the row checkbox being `attached`, not a `visible` `tbody tr`:
    // shared-drive rows can be in the DOM but not "visible" to Playwright,
    // source switches don't reliably re-trigger the loader, and the listing
    // can take well over 30s — so a visibility gate false-negatives.
    const checkbox = this.fileCheckbox(fileName);
    try {
      await checkbox.waitFor({ state: "attached", timeout: 120_000 });
    } catch {
      throw this.fixtureMissingError(fileName, "shared drive panel");
    }
    await checkbox.scrollIntoViewIfNeeded().catch(() => {});
    await checkbox.check({ force: true });
  }

  async selectAction(
    action: "Copy" | "Move",
    _direction?: "egressToNetApp" | "netAppToEgress",
  ): Promise<void> {
    // Direction is implied by the current source (Move renders only when Egress
    // is the source). On the older layout, click "<action> selected" to advance
    // to the destination-tree page (confirmTransfer finishes the choice). On the
    // newer layout there is no such button — the inline "<action> to <folder>"
    // control is clicked in confirmTransfer instead, so this is a no-op.
    const selected = this.selectedControl(action);
    const hasSelected = await selected
      .waitFor({ state: "visible", timeout: 15_000 })
      .then(() => true)
      .catch(() => false);
    if (hasSelected) {
      await selected.click();
    }
  }

  /**
   * Confirm the transfer. No confirm modal on either V1 layout. On the older
   * layout, selectAction advanced to a destination-tree page — pick the first
   * selectable folder (the connected root) and click "<action> to <folder>". On
   * the newer layout, the "<action> to <folder>" button is already inline, so
   * click it directly. Detected by whether the destination tree appears.
   * `action` must match the Copy/Move just initiated (the label depends on it).
   */
  async confirmTransfer(action: "Copy" | "Move"): Promise<void> {
    const tree = this.page.getByRole("tree", { name: "Folders" });
    const onDestinationPage = await tree
      .waitFor({ state: "visible", timeout: 15_000 })
      .then(() => true)
      .catch(() => false);

    if (onDestinationPage) {
      const destination = new TransferDestinationPage(this.page);
      await destination.waitForLoaded();
      await destination.selectFirstSelectableFolder();
      await destination.confirm(action);
    } else {
      await this.inlineTransferControl(action).click();
      // The newer layout raises a confirmation modal after the inline control:
      // tick "I want to move/copy ..." to enable Continue, then confirm.
      const modal = this.page.getByRole("dialog", {
        name: /Transfer confirmation/,
      });
      await modal.waitFor({ state: "visible", timeout: 30_000 });
      await modal
        .getByRole("checkbox", { name: /I want to (move|copy)/i })
        .check();
      await modal.getByRole("button", { name: "Continue" }).click();
    }
  }

  /**
   * Wait for completion: race the (unchanged) success banner against the new
   * error routes, throwing with the page's error text if a transfer fails.
   */
  async waitForTransferComplete(timeout: number = 300_000): Promise<void> {
    const successBanner = this.page.getByTestId(
      "transfer-success-notification-banner",
    );

    await Promise.race([
      successBanner.waitFor({ state: "visible", timeout }),
      this.page.waitForURL(TRANSFER_ERROR_ROUTE, { timeout }),
    ]);

    if (TRANSFER_ERROR_ROUTE.test(this.page.url())) {
      // waitForURL resolves on the URL change, before React renders the error
      // page — so wait for its heading before reading <main>, or innerText()
      // can return the stale transfer-materials <main> (every page wraps its
      // content in <main id="main-content">). Best-effort: the rarer
      // permissions / resolve-file-path routes use a different heading, so the
      // wait lapses and we still surface whatever the page shows.
      await this.page
        .getByRole("heading", {
          name: "There is a problem transferring files",
        })
        .waitFor({ state: "visible", timeout: 15_000 })
        .catch(() => {});
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
    if (!TRANSFER_ERROR_ROUTE.test(this.page.url())) return;
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
    await this.fileCheckbox(fileName).waitFor({ state: "attached", timeout });
  }
}
