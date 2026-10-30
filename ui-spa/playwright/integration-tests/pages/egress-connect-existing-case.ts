import { type Page, expect } from "@playwright/test";
export class EgressConnectExistingCasePage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async verifyUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  async verifyPageElements(searchText: string) {
    await expect(this.page.locator("h1")).toHaveText(
      "Connect Egress to this case",
    );
    await expect(
      this.page.getByRole("button", { name: "Search" }),
    ).toBeVisible();
    await expect(this.page.locator("input").nth(0)).toHaveValue(searchText);
    await expect(this.page.locator("label").nth(0)).toHaveText("Search");
  }

  async verifySearchResult(searchText: string, resultsCount: number) {
    const resultsCountText = `${resultsCount} ${resultsCount === 1 ? "case" : "cases"} found for ${searchText}`;
    await expect(this.page.locator("p").nth(0)).toHaveText(resultsCountText);
  }

  async verifyTableHeaders(expectedHeaders: string[]) {
    const headers = this.page.locator("table thead th");
    await expect(headers).toHaveCount(expectedHeaders.length);

    await Promise.all(
      expectedHeaders.map((header, i) =>
        expect(headers.nth(i)).toHaveText(header),
      ),
    );
  }

  async verifyTableRows(expectedRows: string[][]) {
    const rows = this.page.locator("table tbody tr");
    await expect(rows).toHaveCount(expectedRows.length);

    await Promise.all(
      expectedRows.map((expectedCells, i) => {
        const cells = rows.nth(i).locator("td");
        return Promise.all([
          expect(cells).toHaveCount(expectedCells.length),
          ...expectedCells.map((cell, j) =>
            expect(cells.nth(j)).toHaveText(cell),
          ),
        ]);
      }),
    );
  }

  async verifyAllButtonsDisabled() {
    await Promise.all([
      expect(this.page.getByRole("button", { name: /Search/i })).toBeDisabled(),
      expect(
        this.page
          .locator("table tbody tr")
          .first()
          .getByRole("button", { name: /Connect/i }),
      ).toBeDisabled(),
    ]);
  }

  async clickConnectBtn(rowIndex: number) {
    const rows = this.page.locator("table tbody tr");
    const row = rows.nth(rowIndex);
    const connectBtn = row.getByRole("button", { name: /Connect/i });
    await connectBtn.click();
  }

  async verifyBackLink(url: string) {
    await expect(this.page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(this.page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      url,
    );
  }
  async backLinkClick() {
    await this.page.getByRole("link", { name: "Back" }).click();
  }
  async searchBtnClick() {
    await this.page.getByRole("button", { name: "continue" }).click();
  }
}
