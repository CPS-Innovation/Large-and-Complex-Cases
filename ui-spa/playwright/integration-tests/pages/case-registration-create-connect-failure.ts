import { type Page, expect } from "@playwright/test";
export class CaseRegistrationCreateConnectFailurePage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  getPageContent = (
    action: "create" | "connect",
    type: "egress" | "shared-drive",
  ) => {
    if (action === "create" && type === "egress") {
      return { title: "There is a problem creating a case on Egress" };
    }
    if (action === "connect" && type === "egress") {
      return { title: "There is a problem connecting to Egress" };
    }
    if (action === "create" && type === "shared-drive") {
      return {
        title: "There is a problem creating folders on the Shared Drive",
      };
    }

    return { title: "There is a problem connecting to the Shared Drive" };
  };

  async verifyPageElements(
    action: "create" | "connect",
    type: "egress" | "shared-drive",
  ) {
    const { title } = this.getPageContent(action, type);
    const firstListItemText =
      action === "create"
        ? "return to the previous screen and try again"
        : "return to the previous screen and try to connect again";
    await expect(this.page.locator("h1")).toHaveText(title);
    await expect(
      this.page.getByRole("button", { name: "Try again" }),
    ).toBeVisible();

    await expect(this.page.locator("p").nth(0)).toHaveText("You can:");
    await expect(this.page.locator("li").nth(0)).toHaveText(firstListItemText);
    await expect(this.page.locator("li").nth(1)).toHaveText(
      "contact the product team if you need help",
    );
  }

  async tryAgainBtnClick() {
    await this.page.getByRole("button", { name: "Try again" }).click();
  }
}
