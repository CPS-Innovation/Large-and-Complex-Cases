import { type Page, expect } from "@playwright/test";
export class CaseRegistrationManageSharedDrivePage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl() {
    await expect(this.page).toHaveURL(
      "/case-registration/manage-set-up-shared-drive",
    );
  }

  async errorValidations() {
    await this.clickContinue();
    await expect(
      this.page.getByTestId("manage-shared-drive-error-summary"),
    ).toBeVisible();
    await expect(
      this.page.getByTestId("manage-shared-drive-radio-link"),
    ).toHaveText("Select how you want to manage the Shared Drive");
    await this.page.getByTestId("manage-shared-drive-radio-link").click();
    await expect(
      this.page.locator("#manage-shared-drive-radio-create-new"),
    ).toBeFocused();
  }

  async verifyPageElements() {
    await expect(this.page.locator("h1")).toHaveText(
      "How do you want to manage the Shared Drive?",
    );
    await expect(
      this.page.getByRole("button", { name: "continue" }),
    ).toBeVisible();

    await expect(this.page.locator("label").nth(0)).toHaveText(
      "Create new Shared Drive folders",
    );
    await expect(this.page.locator("label").nth(1)).toHaveText(
      "Connect existing Shared Drive folders to this case",
    );
    await expect(this.page.locator("label").nth(2)).toHaveText(
      "Shared Drive folders not needed",
    );
  }

  async selectCreateNewSharedDrive() {
    await this.page.getByLabel(/^Create new Shared Drive folders$/).check();
  }
  async selectConnectExistingSharedDrive() {
    await this.page
      .getByLabel(/^Connect existing Shared Drive folders to this case$/)
      .check();
  }
  async selectSkipSettingSharedDrive() {
    await this.page.getByLabel(/^Shared Drive folders not needed$/).check();
  }
  async clickContinue() {
    await this.page.getByRole("button", { name: "continue" }).click();
  }
}
