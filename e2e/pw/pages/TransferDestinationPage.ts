import { Page, Locator } from "@playwright/test";

/**
 * New-screen destination page (`/case/:caseId/case-management/transfer-destination-page`).
 *
 * After "Copy selected" / "Move selected" the new screen navigates here instead
 * of showing a confirm modal. The page renders a folder tree (TransferWidget /
 * TreeViewComponent); you expand nodes, pick a target folder, then confirm with
 * a "<Copy|Move> to <folder>" button.
 *
 * Selectors confirmed against the rendered DOM:
 *   - tree:            role="tree", accessible name "Folders"
 *   - folder node:     a <button> whose accessible name is the folder name
 *                      *lowercased* (aria-label = node.name.toLowerCase())
 *   - expand toggle:   sibling <button> with aria-label "plus" (collapsed) /
 *                      "minus" (expanded)
 *   - confirm button:  "<Copy|Move> to <folder>" (folder name in original case);
 *                      reads just "Copy"/"Move" and is disabled until a node is
 *                      selected
 *   - cancel:          <button> "Cancel"
 *   - initial loader:  data-testid "destination-loader"
 */
export class TransferDestinationPage {
  private readonly page: Page;
  private readonly tree: Locator;

  constructor(page: Page) {
    this.page = page;
    this.tree = page.getByRole("tree", { name: "Folders" });
  }

  /** Wait until the folder tree has loaded (initial folder fetch complete). */
  async waitForLoaded(timeout: number = 60_000): Promise<void> {
    await this.page
      .getByTestId("destination-loader")
      .waitFor({ state: "hidden", timeout })
      .catch(() => {});
    await this.tree.waitFor({ state: "visible", timeout });
  }

  /**
   * The folder node button. Its accessible name is the folder name lowercased
   * (aria-label), so match on the lowercased name exactly to avoid substring
   * collisions between sibling folders.
   */
  private folderNode(folderName: string): Locator {
    return this.tree.getByRole("button", {
      name: folderName.toLowerCase(),
      exact: true,
    });
  }

  /** Expand a folder node (no-op if it is already expanded). */
  async expandFolder(folderName: string): Promise<void> {
    // The expand toggle is a sibling button inside the same node container.
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
   * Select the first selectable (non-disabled) folder in the tree and return its
   * accessible name. For Egress→NetApp the root ("Shared drive: …") is selectable
   * and is the connected folder root; for NetApp→Egress the root ("Egress : …")
   * is disabled, so the first enabled node is its first child folder (the root is
   * expanded on load). Handy when the target is just "the connected root folder".
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

  /**
   * Click the confirm button. Once a folder is selected the button reads
   * "<Copy|Move> to <folder>"; it is the only such button on the page, so the
   * "<action> to " prefix identifies it without needing the folder name.
   */
  async confirm(action: "Copy" | "Move"): Promise<void> {
    await this.page
      .getByRole("button", { name: new RegExp(`^${action} to `) })
      .click();
  }

  async cancel(): Promise<void> {
    await this.page.getByRole("button", { name: "Cancel" }).click();
  }

  /**
   * Convenience: open to a target folder and confirm. `folderPath` is the
   * sequence of folder names from a tree root down to the target; ancestors are
   * expanded, the final folder is selected, then the transfer is confirmed.
   */
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
