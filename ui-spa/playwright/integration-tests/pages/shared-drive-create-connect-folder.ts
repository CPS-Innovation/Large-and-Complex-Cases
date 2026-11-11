import { type Page, expect } from "@playwright/test";
import {
  verifyTableHeaders,
  verifyTableRows,
} from "../utils/verifyTableELements";
export class SharedDriveCreateConnectFolderPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  async verifyPageElements(action: "create" | "connect") {
    await expect(this.page.locator("h1")).toHaveText(
      action === "create"
        ? "Choose a location for the new Shared Drive folder"
        : "Connect a Shared Drive folder to this case",
    );
    if (action === "create") {
      await expect(
        this.page.getByText(
          "Select where you want to create the new Shared Drive folder. If you cannot see it, you may need to open other folders first. You'll only see folders you have permission to use.",
        ),
      ).toBeVisible();
    }
    if (action === "connect") {
      await expect(
        this.page.getByText("Select a folder to connect it to this case."),
      ).toBeVisible();
    }
  }

  async verifyTableHeaders(expectedHeaders: string[]) {
    await verifyTableHeaders(this.page, expectedHeaders);
  }

  async verifyTableRows(expectedRows: string[][]) {
    await verifyTableRows(this.page, expectedRows);
  }

  async verifyAllButtonsDisabled(type: "create" | "connect") {
    const btnName = type === "create" ? "Create" : "Connect";
    const rows = this.page.locator("table tbody tr");
    const buttons = rows.getByRole("button", {
      name: new RegExp(btnName, "i"),
    });
    const buttonCount = await buttons.count();

    await Promise.all(
      Array.from({ length: buttonCount }).map((_, i) =>
        expect(buttons.nth(i)).toBeDisabled(),
      ),
    );
  }

  async clickCreateConnectBtn(type: "create" | "connect", rowIndex: number) {
    const rows = this.page.locator("table tbody tr");
    const row = rows.nth(rowIndex);
    const actionBtn = row.getByRole("button", {
      name: new RegExp(type === "create" ? "Create" : "Connect", "i"),
    });
    await actionBtn.click();
  }

  async verifyBackLink(url: string) {
    await expect(this.page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(this.page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      url,
    );
  }
  async backLinkClick() {
    await this.page.getByRole("link", { name: "Back" }).click();
  }
}
