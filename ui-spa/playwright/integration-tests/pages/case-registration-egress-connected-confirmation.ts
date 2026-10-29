import { type Page, expect } from "@playwright/test";
export class CaseRegistrationEgressConnectedConfirmationPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  async verifyPageElements(workspaceName: string = "") {
    const headerText = workspaceName
      ? "Egress case connected"
      : "Egress case created";
    const pText1 = workspaceName
      ? `The Egress case ${workspaceName} has been connected.`
      : "A new case has been created on Egress.";
    await expect(this.page.locator("h1")).toHaveText(headerText);
    await expect(
      this.page.getByRole("button", { name: "continue" }),
    ).toBeVisible();

    await expect(this.page.locator("p").nth(0)).toHaveText(pText1);
    await expect(this.page.locator("p").nth(1)).toHaveText(
      "Select Continue to manage the Shared Drive.",
    );
  }

  async saveAndContinue() {
    await this.page.getByRole("button", { name: "continue" }).click();
  }
}
