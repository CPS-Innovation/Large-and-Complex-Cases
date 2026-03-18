import { Page } from "@playwright/test";

export class NetAppConfirmationPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async confirmConnect() {
    await this.page
      .getByTestId("radio-netapp-connect-yes")
      .check();
    await this.page.locator("button", { hasText: "Continue" }).click();
  }
}
