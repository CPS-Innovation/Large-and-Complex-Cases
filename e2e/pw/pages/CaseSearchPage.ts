import { Page, expect } from "@playwright/test";

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

  // Wait for radios to be *enabled*, not just present. Tactical login must
  // be fresh before the LCC app enables the search form — a stale tactical
  // cookie leaves the radios rendered but disabled, and any submit from
  // that state gets rejected by /api/v1/case-search with HTTP 400.
  async waitForRadioButtons() {
    await Promise.all([
      expect(this.page.getByTestId("radio-search-urn")).toBeEnabled({ timeout: 60_000 }),
      expect(this.page.getByTestId("radio-search-defendant-name")).toBeEnabled({ timeout: 60_000 }),
      expect(this.page.getByTestId("radio-search-operation-name")).toBeEnabled({ timeout: 60_000 }),
    ]);
  }

  async selectUrnSearch() {
    await this.page.getByTestId("radio-search-urn").check();
  }

  async fillUrn(urn: string) {
    await this.page.getByTestId("search-urn").fill(urn);
  }

  async clickSearch() {
    await this.page.getByRole("button", { name: "Search" }).click();
  }

  async searchByUrn(urn: string) {
    await this.goto();
    await this.selectUrnSearch();
    await this.fillUrn(urn);
    await this.clickSearch();
  }
}
