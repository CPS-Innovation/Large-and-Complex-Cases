import { type Page, expect } from "@playwright/test";
export class CaseMaterialsManageEgressPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  async verifyPageElements() {
    await expect(this.page.locator("h1")).toHaveText(
      "How do you want to manage Egress?",
    );
    await expect(
      this.page.getByRole("button", { name: "continue" }),
    ).toBeVisible();

    await expect(this.page.locator("label").nth(0)).toHaveText(
      "Create a new case on Egress",
    );
    await expect(this.page.locator("label").nth(1)).toHaveText(
      "Connect an existing Egress case",
    );
    await expect(this.page.locator("label").nth(2)).toHaveText(
      "Egress is not needed now",
    );
  }
  async errorValidations() {
    await this.clickContinue();
    await expect(
      this.page.getByTestId("manage-egress-error-summary"),
    ).toBeVisible();
    await expect(this.page.getByTestId("manage-egress-radio-link")).toHaveText(
      "Select how you want to manage Egress",
    );
    await this.page.getByTestId("manage-egress-radio-link").click();
    await expect(
      this.page.locator("#manage-egress-radio-create-new"),
    ).toBeFocused();
  }

  async selectCreateNewEgress() {
    await this.page.getByLabel(/^Create a new case on Egress$/).check();
  }
  async selectConnectExistingEgress() {
    await this.page.getByLabel(/^Connect an existing Egress case$/).check();
  }
  async selectSkipSettingEgress() {
    await this.page.getByLabel(/^Egress is not needed now$/).check();
  }
  async clickContinue() {
    await this.page.getByRole("button", { name: "continue" }).click();
  }
}
