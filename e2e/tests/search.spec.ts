import { test, expect } from "@playwright/test";
import { HomePage } from "../pages/HomePage";
import { SearchResultsPage } from "../pages/SearchResultsPage";

test.describe("Search", () => {
  test("Can search via Urn", async ({ page, baseURL }) => {
    const homePage = new HomePage(page);
    const resultsPage = new SearchResultsPage(page);
    await homePage.goto();

    await homePage.selectRadio("urn");

    const urn = "01AA0101010";
    await homePage.urn.fill(urn);
    await homePage.button.click();

    const results = await resultsPage.results(
      "Defendant or operation name",
      "URN"
    );

    expect(results).toEqual([
      { "Defendant or operation name": "Alpha", URN: urn },
    ]);
  });

  test("Can search by defendant name", async ({ page }) => {
    const homePage = new HomePage(page);
    const resultsPage = new SearchResultsPage(page);
    await homePage.goto();

    const defendantName = "charlton";
    await homePage.selectRadio("defendant-name");
    await homePage.defendantName.fill(defendantName);
    await homePage.selectArea("defendant", "1001");
    await homePage.button.click();

    const results = await resultsPage.results(
      "Defendant or operation name",
      "URN"
    );

    expect(results).toEqual([
      { "Defendant or operation name": "Charlie", URN: "03CC0303030" },
      { "Defendant or operation name": "Hotel", URN: "08HH0808080" },
    ]);
  });

  test("Can search by operation name", async ({ page }) => {
    const homePage = new HomePage(page);
    const resultsPage = new SearchResultsPage(page);
    await homePage.goto();

    const operationName = "Foxtrot";
    await homePage.selectRadio("operation-name");
    await homePage.operationName.fill(operationName);
    await homePage.selectArea("operation", "1001");
    await homePage.button.click();

    const results = await resultsPage.results(
      "Defendant or operation name",
      "URN"
    );

    expect(results).toEqual([
      { "Defendant or operation name": operationName, URN: "06FF0606060" },
    ]);
  });
});
