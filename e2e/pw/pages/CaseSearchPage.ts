import { Page, expect } from "@playwright/test";

export class CaseSearchPage {
  static readonly route = "/";
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    const t0 = Date.now();

    console.log("BEFORE GOTO");

    await this.page.goto(CaseSearchPage.route, {
      waitUntil: "commit"
    });

    console.log("AFTER GOTO", Date.now() - t0);

    const locator = this.page.getByTestId(
      "radio-search-operation-name"
    );

    console.log("BEFORE EXPECT");

    await expect(locator).toBeEnabled({
      timeout: 60_000
    });

    console.log("AFTER EXPECT", Date.now() - t0);
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
    this.page.on("request", r => console.log("→", r.method(), r.url()));
    this.page.on("response", r => console.log("←", r.status(), r.url()));
    await this.goto();
    await this.selectUrnSearch();
    await this.fillUrn(urn);
    await this.clickSearch();
  }
}
