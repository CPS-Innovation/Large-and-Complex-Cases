import { Page, Locator } from "@playwright/test";

/**
 * New-screen destination page (`.../case-management/transfer-destination-page`),
 * reached after "Copy selected" / "Move selected" instead of a confirm modal.
 * Renders a folder tree; you expand nodes, pick a target, then confirm with a
 * "<Copy|Move> to <folder>" button.
 *
 * DOM: tree = role "tree" name "Folders"; folder node = a <button> whose
 * aria-label is the folder name *lowercased*; expand toggle = sibling button
 * aria-label "plus"/"minus"; confirm = "<Copy|Move> to <folder>" (disabled
 * until a node is selected); initial loader = testid "destination-loader".
 */
export class TransferDestinationPage {
  private readonly page: Page;
  private readonly tree: Locator;

  constructor(page: Page) {
    this.page = page;
    this.tree = page.getByRole("tree", { name: "Folders" });
  }

  /** Wait for the folder tree to finish its initial load. */
  async waitForLoaded(timeout: number = 60_000): Promise<void> {
    await this.page
      .getByTestId("destination-loader")
      .waitFor({ state: "hidden", timeout })
      .catch(() => {});
    await this.tree.waitFor({ state: "visible", timeout });
  }

  /** Folder node button — aria-label is the lowercased name; match exactly to
   * avoid sibling substring collisions. */
  private folderNode(folderName: string): Locator {
    return this.tree.getByRole("button", {
      name: folderName.toLowerCase(),
      exact: true,
    });
  }

  /** Expand a folder node (no-op if already expanded). */
  async expandFolder(folderName: string): Promise<void> {
    const toggle = this.folderNode(folderName)
      .locator("xpath=..")
      .getByRole("button", { name: "plus" });
    if ((await toggle.count()) > 0) {
      await toggle.first().click();
    }
  }

  /** Select (highlight) a folder node as the transfer target. */
  async selectFolder(folderName: string): Promise<void> {
    await this.folderNode(folderName).click();
  }

  /**
   * Select the first enabled folder and return its label. For Egress→NetApp the
   * root is selectable (the connected folder); for NetApp→Egress the root is
   * disabled so this picks its first child. Use when the target is just "the
   * connected root".
   */
  async selectFirstSelectableFolder(): Promise<string> {
    const folders = this.tree.locator(
      'button[aria-label]:not([aria-label="plus"]):not([aria-label="minus"])',
    );
    const count = await folders.count();
    for (let i = 0; i < count; i++) {
      const btn = folders.nth(i);
      if (await btn.isEnabled()) {
        const label = (await btn.getAttribute("aria-label")) ?? "";
        await btn.click();
        return label;
      }
    }
    throw new Error("No selectable folder found in the destination tree");
  }

  /** Click confirm. Once a folder is selected the button reads
   * "<Copy|Move> to <folder>" and is the only such button. */
  async confirm(action: "Copy" | "Move"): Promise<void> {
    await this.page
      .getByRole("button", { name: new RegExp(`^${action} to `) })
      .click();
  }

  async cancel(): Promise<void> {
    await this.page.getByRole("button", { name: "Cancel" }).click();
  }

  /** Expand ancestors, select the final folder, and confirm. `folderPath` runs
   * from a tree root down to the target. */
  async chooseFolder(
    action: "Copy" | "Move",
    folderPath: string[],
  ): Promise<void> {
    await this.waitForLoaded();
    for (const ancestor of folderPath.slice(0, -1)) {
      await this.expandFolder(ancestor);
    }
    const target = folderPath[folderPath.length - 1];
    await this.selectFolder(target);
    await this.confirm(action);
  }
}
