import { type Page, expect } from "@playwright/test";

export class TransferMaterialsDestinationPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  async verifyPageElements(
    transferSource: "egress" | "shared-drive",
    itemsCount: number,
    transferType: "copy" | "move",
  ) {
    await expect(this.page.locator("h1")).toHaveText(
      `Choose ${transferSource === "shared-drive" ? "an Egress" : "a Shared Drive"} folder`,
    );
    await expect(this.page.locator("p").first()).toHaveText(
      `You are ${transferType === "copy" ? "copying" : "moving"} ${itemsCount} item${itemsCount > 1 ? "s" : ""}.`,
    );
    await expect(this.page.locator("p").nth(1)).toHaveText(
      `Select the ${transferSource === "shared-drive" ? "Egress" : "Shared Drive"} folder you want to ${transferType} them into.`,
    );
    await expect(this.page.locator("h2")).toHaveText("Transfer Files");
  }

  async verifyTreeItems(expectedItems: string[]) {
    const treeItems = this.page.locator("[role='treeitem']");
    await expect(treeItems).toHaveCount(expectedItems.length);
    for (const [index, item] of expectedItems.entries()) {
      await expect(treeItems.nth(index).getByRole("button").nth(1)).toHaveText(
        item,
      );
    }
  }

  async verifyTreeItemsCount(count: number) {
    const treeItems = this.page.locator("[role='treeitem']");
    await expect(treeItems).toHaveCount(count);
  }

  async openFolder(itemName: string) {
    const treeItems = this.page.locator("[role='treeitem']");
    await treeItems.locator("text=" + itemName).click();
  }

  async clickExpandFolder(itemName: string) {
    const treeItem = await this.getImmediateParentTreeItem(itemName);

    const plusToggle = treeItem.locator('button[aria-label="plus"]');
    await plusToggle.first().click();
  }

  async clickMinimizeFolder(itemName: string) {
    const treeItem = await this.getImmediateParentTreeItem(itemName);

    const minusToggle = treeItem.locator('button[aria-label="minus"]');
    await minusToggle.first().click();
  }

  async selectFolder(folderName: string) {
    await this.page.getByRole("button", { name: folderName }).click();
  }

  async verifyTransferActionButtonName(name: string) {
    await expect(this.page.getByTestId("transfer-action-button")).toHaveText(
      name,
    );
  }

  async verifyTransferActionEnabled(isEnabled: boolean) {
    const button = this.page.getByTestId("transfer-action-button");
    if (isEnabled) {
      await expect(button).toBeEnabled();
    } else {
      await expect(button).toBeDisabled();
    }
  }

  async clickTransferActionButton() {
    await this.page.getByTestId("transfer-action-button").click();
  }

  async clickCancelButton() {
    await this.page.getByRole("button", { name: "Cancel" }).click();
  }
  async clickBackLink() {
    await this.page.getByRole("link", { name: "Back" }).click();
  }
  async getImmediateParentTreeItem(folderName: string) {
    const folderButton = this.page.getByRole("button", { name: folderName });
    // select the closest ancestor <li role="treeitem">
    return folderButton.locator('xpath=ancestor::li[@role="treeitem"][1]');
  }

  async verifyFolderExpanded(
    folderName: string,
    expanded: boolean,
    childItems: string[] = [],
  ) {
    const treeItem = await this.getImmediateParentTreeItem(folderName);
    if (expanded) {
      await expect(
        treeItem.locator('button[aria-label="minus"]'),
      ).toBeVisible();
      await expect(treeItem).toHaveAttribute("aria-expanded", "true");

      const childTreeItems = treeItem.locator("[role='treeitem']");
      await expect(childTreeItems).toHaveCount(childItems.length);
      for (const [index, item] of childItems.entries()) {
        await expect(
          childTreeItems.nth(index).getByRole("button").nth(1),
        ).toHaveText(item);
      }
    } else {
      await expect(treeItem.locator('button[aria-label="plus"]')).toBeVisible();
      await expect(treeItem).toHaveAttribute("aria-expanded", "false");
      const childTreeItems = treeItem.locator("[role='treeitem']");
      await expect(childTreeItems).toHaveCount(0);
    }
  }

  async verifyDisabledTreeItem(itemName: string) {
    const treeItem = await this.getImmediateParentTreeItem(itemName);
    await expect(treeItem).toHaveClass(/disabled/);
    await expect(
      this.page.getByRole("button", { name: itemName }),
    ).toBeDisabled();
  }

  async verifyTransferDestinationTableLoader(visible: boolean) {
    if (visible) {
      await expect(this.page.getByTestId("loading-spinner")).toBeVisible({
        timeout: 30000,
      });

      return;
    }
    await expect(this.page.getByTestId("loading-spinner")).not.toBeVisible({
      timeout: 30000,
    });
  }
}
