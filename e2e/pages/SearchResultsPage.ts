import { Page } from "@playwright/test";

export class SearchResultsPage {
  private readonly page: Page;
  constructor(page: Page) {
    this.page = page;
  }

  async results(...columns: string[]): Promise<Record<string, string>[]> {
    await this.page.waitForSelector("table");
    const headerNames = await Promise.all(
      (
        await this.page.locator("table thead th").all()
      ).map((l) => l.innerText())
    );

    const results = [];

    const rows = await this.page.locator("table tbody tr").all();
    for (const row of rows) {
      const rowValues = (await row.locator("td").all()).map((l) =>
        l.innerText()
      );
      const result = {} as Record<string, string>;
      results.push(result);
      for (let i = 0; i <= rowValues.length; i++) {
        if (columns.includes(headerNames[i])) {
          result[headerNames[i]] = await rowValues[i];
        }
      }
    }

    return results;
  }
}
