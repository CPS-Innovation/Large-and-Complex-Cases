import { type Page, expect } from "@playwright/test";
export class CaseRegistrationSharedDriveTemplatesPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyPageElements(templates: string[]) {
    await expect(this.page.locator("h1")).toHaveText(
      "Select a Shared Drive folder template",
    );

    await Promise.all(
      templates.map((template, index) =>
        expect(this.page.locator("label").nth(index)).toHaveText(template),
      ),
    );

    await expect(
      this.page.getByRole("button", { name: "continue" }),
    ).toBeVisible();
  }

  async pageErrorValidations() {
    await this.clickContinue();
    await expect(
      this.page.getByTestId("shared-drive-templates-error-summary"),
    ).toBeVisible();
    await expect(
      this.page.getByTestId("shared-drive-template-radio-link"),
    ).toHaveText("Select which template you want to use");
    await this.page.getByTestId("shared-drive-template-radio-link").click();
    await expect(
      this.page.locator("#shared-drive-template-radio-template-0"),
    ).toBeFocused();
  }

  async verifyUrl() {
    await expect(this.page).toHaveURL(
      "/case-registration/shared-drive-templates",
    );
  }

  async selectSharedDriveTemplate(name: string) {
    await this.page.getByLabel(new RegExp(name)).check();
  }

  async clickContinue() {
    await this.page.getByRole("button", { name: "continue" }).click();
  }
}
