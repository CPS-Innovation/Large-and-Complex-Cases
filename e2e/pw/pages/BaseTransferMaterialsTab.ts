import { Page } from "@playwright/test";

/**
 * Shared Egress-side and common helpers for both Transfer Materials page
 * objects. The Egress table, folder navigation, file-indexing wait and the
 * NetApp date-sort are identical across the old and new screens (bar the NetApp
 * table's test id), so they live here; screen-specific behaviour stays in the
 * `TransferMaterialsTab` / `TransferMaterialsTabV1` subclasses.
 */
export abstract class BaseTransferMaterialsTab {
  protected readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  /** Wait for the Egress / NetApp panel's folder listing to finish loading. */
  abstract waitForEgressFiles(): Promise<void>;
  abstract waitForNetAppFiles(): Promise<void>;

  /** Test id of the NetApp / shared-drive table wrapper on this screen. */
  protected abstract readonly netAppWrapperTestId: string;

  async selectAllEgressFiles(): Promise<void> {
    await this.page
      .getByTestId("egress-table-wrapper")
      .getByLabel("Select folders and files")
      .check();
  }

  async selectEgressFileByName(fileName: string): Promise<void> {
    const row = this.page
      .getByTestId("egress-table-wrapper")
      .locator("tbody tr", { hasText: fileName });
    const checkbox = row.locator('input[type="checkbox"]');
    await checkbox.scrollIntoViewIfNeeded();
    await checkbox.check();
  }

  async navigateToFolder(folderName: string): Promise<void> {
    await this.page.getByRole("button", { name: folderName }).click();
  }

  /**
   * Sort the NetApp panel by last-modified date descending (two header clicks).
   * Best-effort: the sort doesn't reliably toggle to descending, so a flaky/slow
   * header is tolerated and the panel is left unsorted.
   */
  async sortNetAppByDateDescending(): Promise<void> {
    const header = this.page
      .getByTestId(this.netAppWrapperTestId)
      .getByRole("button", { name: "Last modified date" });
    try {
      await header.click({ timeout: 20_000 });
      await this.waitForNetAppFiles();
      await header.click({ timeout: 20_000 });
      await this.waitForNetAppFiles();
    } catch {
      console.log(
        "  [sortNetAppByDateDescending] date sort skipped (best-effort)",
      );
    }
  }

  /**
   * Wait for a file to appear in the Egress panel. Egress fetches a folder's
   * list once and doesn't auto-refresh, so each retry reloads (busting the
   * React Query cache) and re-navigates the folder path.
   */
  async waitForEgressFileByName(
    fileName: string,
    folderPath: string[],
    timeout: number = 300_000,
  ): Promise<void> {
    const egressTable = this.page.getByTestId("egress-table-wrapper");
    const start = Date.now();

    const fileVisible = async () =>
      (await egressTable.locator("tbody tr", { hasText: fileName }).count()) >
      0;

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

  /** Standard "fixture missing" error for the NetApp / shared-drive panel. */
  protected fixtureMissingError(fileName: string, panelName: string): Error {
    return new Error(
      `Required NetApp fixture '${fileName}' not found in the ${panelName}.\n` +
        `  Seed it via:\n` +
        `    bash:       RUN_SEED=1 npx playwright test --project=seed-netapp-fixture\n` +
        `    powershell: $env:RUN_SEED=1; npx playwright test --project=seed-netapp-fixture\n` +
        `  See README "Required NetApp fixture" for details.`,
    );
  }
}
