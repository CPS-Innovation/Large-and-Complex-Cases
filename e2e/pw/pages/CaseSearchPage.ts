import { Page } from "@playwright/test";

export class CaseSearchPage {
  static readonly route = "/";
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto(CaseSearchPage.route);
    await this.page.waitForSelector("h1");
    await this.waitForRadioButtons();
  }

  async waitForRadioButtons() {
    await Promise.all([
      this.page.getByTestId("radio-search-urn").waitFor({ state: "visible", timeout: 30000 }),
      this.page.getByTestId("radio-search-defendant-name").waitFor({ state: "visible", timeout: 30000 }),
      this.page.getByTestId("radio-search-operation-name").waitFor({ state: "visible", timeout: 30000 }),
    ]);
  }

  async selectUrnSearch() {
    await this.page.getByTestId("radio-search-urn").check();
  }

  async fillUrn(urn: string) {
    await this.page.getByTestId("search-urn").fill(urn);
  }

  async clickSearch() {
    await this.page.locator("button").click();
  }

  async searchByUrn(urn: string) {
    await this.goto();
    await this.selectUrnSearch();
    await this.fillUrn(urn);
    await this.clickSearch();
  }
}
