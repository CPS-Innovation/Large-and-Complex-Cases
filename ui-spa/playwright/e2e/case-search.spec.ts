import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";

test.describe("Case Search", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
  });
  test("case search by operation name and area", async ({ page }) => {
    await page.getByRole("radio", { name: "Operation name" }).check();
    const input = await page.getByTestId("search-operation-name");
    await expect(input).toBeVisible();
    expect(await page.getByTestId("search-defendant-name")).not.toBeVisible();
    expect(await page.getByTestId("search-defendant-area")).not.toBeVisible();
    expect(await page.getByTestId("search-urn")).not.toBeVisible();
    await input.fill("thunder");
    const areaSelect = await page.getByTestId("search-operation-area");
    const options = await areaSelect.locator("option").allTextContents();
    await expect(options).toEqual([
      "-- Please select --",
      "SEOCID Int London and SE Div",
      "Special Crime Division",
      "Bedfordshire",
      "Cambridgeshire",
      "Cheshire",
    ]);
    areaSelect.selectOption("Cambridgeshire");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL(
      /\/search-results\?operation-name=thunder&area=4/,
    );
    await expect(page.locator("h1")).toHaveText(
      "Search for Operation name search",
    );
    await expect(
      page.getByText("We've found 2 results for thunder in Cambridgeshire."),
    ).toBeVisible();

    await expect(page.getByTestId("search-operation-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-operation-area")).toHaveValue("4");
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual([
      "Thunderstruck1_pl",
      "ABCDEF1",
      "abc1",
      "Connected",
      "Inactive",
      "02/01/2000",
      "View ",
    ]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual([
      "Thunderstruck2_pl",
      "ABCDEF2",
      "abc2",
      "Connected",
      "Connected",
      "03/01/2000",
      "View ",
    ]);
  });

  test("case search by defendant surname and area", async ({ page }) => {
    await page.getByRole("radio", { name: "Defendant surname" }).check();
    await expect(
      page.getByRole("radio", { name: "Defendant surname" }),
    ).toBeChecked();
    const input = await page.getByTestId("search-defendant-name");
    await expect(input).toBeVisible();
    expect(await page.getByTestId("search-operation-name")).not.toBeVisible();
    expect(await page.getByTestId("search-operation-area")).not.toBeVisible();
    expect(await page.getByTestId("search-urn")).not.toBeVisible();
    await input.fill("thunder");
    const areaSelect = await page.getByTestId("search-defendant-area");
    const options = await areaSelect.locator("option").allTextContents();
    await expect(options).toEqual([
      "-- Please select --",
      "SEOCID Int London and SE Div",
      "Special Crime Division",
      "Bedfordshire",
      "Cambridgeshire",
      "Cheshire",
    ]);
    areaSelect.selectOption("Cambridgeshire");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL(
      /\/search-results\?defendant-name=thunder&area=4/,
    );
    await expect(page.locator("h1")).toHaveText(
      "Search for Defendant surname search",
    );
    await expect(
      page.getByText("We've found 2 results for thunder in Cambridgeshire."),
    ).toBeVisible();

    await expect(page.getByTestId("search-defendant-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-defendant-area")).toHaveValue("4");
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual([
      "Thunderstruck1_pl",
      "ABCDEF1",
      "abc1",
      "Connected",
      "Inactive",
      "02/01/2000",
      "View ",
    ]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual([
      "Thunderstruck2_pl",
      "ABCDEF2",
      "abc2",
      "Connected",
      "Connected",
      "03/01/2000",
      "View ",
    ]);
  });

  test("case search by urn", async ({ page }) => {
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    expect(await page.getByTestId("search-operation-name")).not.toBeVisible();
    expect(await page.getByTestId("search-operation-area")).not.toBeVisible();
    expect(await page.getByTestId("search-defendant-name")).not.toBeVisible();
    expect(await page.getByTestId("search-defendant-area")).not.toBeVisible();

    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL(/\/search-results\?urn=11AA2222233/);
    await expect(page.locator("h1")).toHaveText("Search for URN search");
    await expect(
      page.getByText("We've found 2 results for 11AA2222233."),
    ).toBeVisible();

    await expect(page.getByTestId("search-urn")).toHaveValue("11AA2222233");

    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual([
      "Thunderstruck1_pl",
      "ABCDEF1",
      "abc1",
      "Connected",
      "Inactive",
      "02/01/2000",
      "View ",
    ]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual([
      "Thunderstruck2_pl",
      "ABCDEF2",
      "abc2",
      "Connected",
      "Connected",
      "03/01/2000",
      "View ",
    ]);
  });
});

test.describe("Case Search Results", () => {
  test("should show the search results if user lands directly on the search results page with valid search by operation name params", async ({
    page,
  }) => {
    await page.goto("/search-results?operation-name=thunder&area=4");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/search-results?*`);
    const areaSelect = await page.getByTestId("search-operation-area");
    const options = await areaSelect.locator("option").allTextContents();
    await expect(options).toEqual([
      "-- Please select --",
      "SEOCID Int London and SE Div",
      "Special Crime Division",
      "Bedfordshire",
      "Cambridgeshire",
      "Cheshire",
    ]);
    await expect(page.locator("h1")).toHaveText(
      "Search for Operation name search",
    );
    await expect(
      page.getByText("We've found 2 results for thunder in Cambridgeshire."),
    ).toBeVisible();

    await expect(page.getByTestId("search-operation-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-operation-area")).toHaveValue("4");
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual([
      "Thunderstruck1_pl",
      "ABCDEF1",
      "abc1",
      "Connected",
      "Inactive",
      "02/01/2000",
      "View ",
    ]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual([
      "Thunderstruck2_pl",
      "ABCDEF2",
      "abc2",
      "Connected",
      "Connected",
      "03/01/2000",
      "View ",
    ]);
  });

  test("should show the search results if user lands directly on the search results page with valid search by defendant surname params", async ({
    page,
  }) => {
    await page.goto("/search-results?defendant-name=thunder&area=4");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/search-results?*`);
    const areaSelect = await page.getByTestId("search-defendant-area");
    const options = await areaSelect.locator("option").allTextContents();
    await expect(options).toEqual([
      "-- Please select --",
      "SEOCID Int London and SE Div",
      "Special Crime Division",
      "Bedfordshire",
      "Cambridgeshire",
      "Cheshire",
    ]);
    await expect(page.locator("h1")).toHaveText(
      "Search for Defendant surname search",
    );
    await expect(
      page.getByText("We've found 2 results for thunder in Cambridgeshire."),
    ).toBeVisible();

    await expect(page.getByTestId("search-defendant-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-defendant-area")).toHaveValue("4");
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual([
      "Thunderstruck1_pl",
      "ABCDEF1",
      "abc1",
      "Connected",
      "Inactive",
      "02/01/2000",
      "View ",
    ]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual([
      "Thunderstruck2_pl",
      "ABCDEF2",
      "abc2",
      "Connected",
      "Connected",
      "03/01/2000",
      "View ",
    ]);
  });

  test("should show the search results if user lands directly on the search results page with valid search by URN param", async ({
    page,
  }) => {
    await page.goto("/search-results?urn=11AA2222233");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/search-results?*`);
    await expect(page.locator("h1")).toHaveText("Search for URN search");
    await expect(
      page.getByText("We've found 2 results for 11AA2222233."),
    ).toBeVisible();

    await expect(page.getByTestId("search-urn")).toHaveValue("11AA2222233");

    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual([
      "Thunderstruck1_pl",
      "ABCDEF1",
      "abc1",
      "Connected",
      "Inactive",
      "02/01/2000",
      "View ",
    ]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual([
      "Thunderstruck2_pl",
      "ABCDEF2",
      "abc2",
      "Connected",
      "Connected",
      "03/01/2000",
      "View ",
    ]);
  });

  test("Should show no cases found result, if there are no matching result for given search criteria", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/search-results", async () => {
        await delay(10);
        return HttpResponse.json([]);
      }),
    );
    await page.goto("/search-results?urn=11AA2222233");

    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/search-results?*`);

    await expect(page.locator("h1")).toHaveText("Search for URN search");
    await expect(
      page.getByText("There are no matching results for 11AA2222233."),
    ).toBeVisible();
    await expect(
      page.getByText("There are no matching results for 11AA2222233."),
    ).toBeVisible();

    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(4);
    await expect(listItems).toHaveText([
      "check CMS to see if the case exists",
      "check the spelling of your search",
      "check with your Unit Manager to see if the case is restricted",
      "contact the Service Desk",
    ]);
  });
});
