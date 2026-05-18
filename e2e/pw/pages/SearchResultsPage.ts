import { Page } from "@playwright/test";

export class SearchResultsPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async waitForResults() {
    await this.page.waitForSelector("table");
  }

  async getResultRows(
    ...columns: string[]
  ): Promise<Record<string, string>[]> {
    await this.waitForResults();

    const headerNames = await Promise.all(
      (await this.page.locator("table thead th").all()).map((l) =>
        l.innerText()
      )
    );

    const results: Record<string, string>[] = [];
    const rows = await this.page.locator("table tbody tr").all();

    for (const row of rows) {
      const cells = await row.locator("td").all();
      const cellValues = await Promise.all(cells.map((c) => c.innerText()));
      const result: Record<string, string> = {};

      for (let i = 0; i < cellValues.length; i++) {
        if (columns.length === 0 || columns.includes(headerNames[i])) {
          result[headerNames[i]] = cellValues[i];
        }
      }
      results.push(result);
    }

    return results;
  }

  async clickCaseAction(urn: string) {
    const row = this.page.locator("table tbody tr", { hasText: urn });
    await row.locator("a").first().click();
  }
}
