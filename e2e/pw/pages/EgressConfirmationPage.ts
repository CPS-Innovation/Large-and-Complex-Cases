import { Page } from "@playwright/test";

export class EgressConfirmationPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async confirmConnect() {
    await this.page
      .getByTestId("radio-egress-connect-yes")
      .check();
    await this.page.locator("button", { hasText: "Continue" }).click();
  }
}
