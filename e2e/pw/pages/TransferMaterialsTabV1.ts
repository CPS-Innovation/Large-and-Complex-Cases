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
  // Remembered so confirmTransfer knows whether the destination-page button
  // reads "Copy to …" or "Move to …".
  private lastAction: "Copy" | "Move" = "Copy";

  /** Row checkbox for a shared-drive file, by its exact aria-label. */
  private fileCheckbox(fileName: string): Locator {
    return this.page
      .locator(`input[type="checkbox"][aria-label="select file ${fileName}"]`)
      .first();
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

    await Promise.race([
      successBanner.waitFor({ state: "visible", timeout }),
      this.page.waitForURL(TRANSFER_ERROR_ROUTE, { timeout }),
    ]);

    if (TRANSFER_ERROR_ROUTE.test(this.page.url())) {
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
