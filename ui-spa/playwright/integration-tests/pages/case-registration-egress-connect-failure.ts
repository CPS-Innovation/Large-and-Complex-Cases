import { type Page, expect } from "@playwright/test";
export class CaseRegistrationEgressConnectFailurePage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl() {
    await expect(this.page).toHaveURL(
      "/case-registration/connect-egress-case-error",
    );
  }

  async verifyPageElements() {
    await expect(this.page.locator("h1")).toHaveText(
      "There is a problem connecting to Egress",
    );
    await expect(
      this.page.getByRole("button", { name: "Try again" }),
    ).toBeVisible();

    await expect(this.page.locator("p").nth(0)).toHaveText("You can:");
    await expect(this.page.locator("li").nth(0)).toHaveText(
      "return to the previous screen and try to connect again",
    );
    await expect(this.page.locator("li").nth(1)).toHaveText(
      "contact the product team if you need help",
    );
  }

  async tryAgainBtnClick() {
    await this.page.getByRole("button", { name: "Try again" }).click();
  }
}
