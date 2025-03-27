import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";
import { caseAreasPlaywright } from "../../src/mocks/data";

test.describe("Case Search", async () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
  });
  test("Should successfully complete the case search by operation name and area and see the results", async ({
    page,
  }) => {
    await page.getByRole("radio", { name: "Operation name" }).check();
    const input = await page.getByTestId("search-operation-name");
    await expect(input).toBeVisible();
    expect(await page.getByTestId("search-defendant-name")).not.toBeVisible();
    expect(await page.getByTestId("search-defendant-area")).not.toBeVisible();
    expect(await page.getByTestId("search-urn")).not.toBeVisible();
    await input.fill("thunder");
    const areaSelect = await page.getByTestId("search-operation-area");
    const options = await areaSelect.locator("option").allTextContents();
    expect(options).toHaveLength(51);
    await expect(areaSelect).toHaveValue("1057708");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL(
      /\/search-results\?operation-name=thunder&area=1057708/,
    );
    await expect(page.locator("h1")).toHaveText(
      `Search results for operation "thunder"`,
    );
    await expect(
      page.getByText(
        "2 cases found in Organised Crime Division. Select a case to view more details.",
      ),
    ).toBeVisible();

    await expect(page.getByTestId("search-operation-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-operation-area")).toHaveValue(
      "1057708",
    );
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

  test("Should successfully complete the case search case search by defendant surname and area and see the results", async ({
    page,
  }) => {
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
    expect(options).toHaveLength(51);
    await expect(areaSelect).toHaveValue("1057708");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL(
      "search-results?defendant-name=thunder&area=1057708",
    );
    await expect(page.locator("h1")).toHaveText(
      `Search results for defendant surname "thunder"`,
    );
    await expect(
      page.getByText(
        "2 cases found in Organised Crime Division. Select a case to view more details.",
      ),
    ).toBeVisible();

    await expect(page.getByTestId("search-defendant-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-defendant-area")).toHaveValue(
      "1057708",
    );
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

  test("Should successfully complete the case search case search by urn and see the results", async ({
    page,
  }) => {
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    expect(await page.getByTestId("search-operation-name")).not.toBeVisible();
    expect(await page.getByTestId("search-operation-area")).not.toBeVisible();
    expect(await page.getByTestId("search-defendant-name")).not.toBeVisible();
    expect(await page.getByTestId("search-defendant-area")).not.toBeVisible();

    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("2 cases found. Select a case to view more details."),
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

  test("should show validation error for search by urn in the case search page", async ({
    page,
  }) => {
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    await expect(page.getByTestId("search-error-summary")).not.toBeVisible();
    await page.locator('button:text("search")').click();
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page.getByTestId("search-urn-link").getByText("URN should not be empty"),
    ).toBeVisible();

    page.getByTestId("search-urn-link").click();
    await expect(page.getByTestId("search-urn")).toBeFocused();

    await page.getByTestId("search-urn").fill("www");
    await page.locator('button:text("search")').click();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-urn-link")
        .getByText("Enter a valid unique reference number"),
    ).toBeVisible();
    page.getByTestId("search-urn-link").click();
    await expect(page.getByTestId("search-urn")).toBeFocused();
  });

  test("should show validation error for search by operation name in the case search page", async ({
    page,
    worker,
  }) => {
    const emptyHomeAreaResponse = { ...caseAreasPlaywright, homeArea: {} };
    await worker.use(
      http.get("https://mocked-out-api/api/areas", async () => {
        await delay(10);
        return HttpResponse.json(emptyHomeAreaResponse);
      }),
    );
    await page.goto("/");
    await page.getByRole("radio", { name: "Operation name" }).check();
    await expect(page.getByTestId("search-error-summary")).not.toBeVisible();
    await page.locator('button:text("search")').click();
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-operation-name-link")
        .getByText("Operation name should not be empty"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-operation-area-link")
        .getByText("Operation area should not be empty"),
    ).toBeVisible();
    page.getByTestId("search-operation-name-link").click();
    await expect(page.getByTestId("search-operation-name")).toBeFocused();
    page.getByTestId("search-operation-area-link").click();
    await expect(page.getByTestId("search-operation-area")).toBeFocused();
  });

  test("should show validation error for search by defendant surname in the case search page", async ({
    page,
    worker,
  }) => {
    const emptyHomeAreaResponse = { ...caseAreasPlaywright, homeArea: {} };
    await worker.use(
      http.get("https://mocked-out-api/api/areas", async () => {
        await delay(10);
        return HttpResponse.json(emptyHomeAreaResponse);
      }),
    );
    await page.goto("/");
    await page.getByRole("radio", { name: "Defendant surname" }).check();
    await expect(page.getByTestId("search-error-summary")).not.toBeVisible();
    await page.locator('button:text("search")').click();
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-defendant-name-link")
        .getByText("Defendant surname should not be empty"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-defendant-area-link")
        .getByText("Defendant area should not be empty"),
    ).toBeVisible();
    page.getByTestId("search-defendant-name-link").click();
    await expect(page.getByTestId("search-defendant-name")).toBeFocused();
    page.getByTestId("search-defendant-area-link").click();
    await expect(page.getByTestId("search-defendant-area")).toBeFocused();
  });

  test("Should be able to submit the form using enter button", async ({
    page,
  }) => {
    await page.getByTestId("search-urn").fill("11AA2222233");
    await page.getByTestId("search-urn").press("Enter");
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
  });

  test("Should disable the search by operation name and search by defendant surname options if areas api failed but users should be able to do the URN search", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/areas", async () => {
        await delay(10);
        return new HttpResponse(null, { status: 500 });
      }),
    );
    await page.goto("/");
    await expect(
      page.getByRole("radio", { name: "Operation name" }),
    ).toBeDisabled();
    await expect(
      page.getByRole("radio", { name: "Defendant surname" }),
    ).toBeDisabled();
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    await page.getByTestId("search-urn").fill("11AA2222233");
    await page.getByTestId("search-urn").press("Enter");
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("2 cases found. Select a case to view more details."),
    ).toBeVisible();
  });
});

test.describe("Case Search Results", () => {
  test("should show the search results if user lands directly on the search results page with valid search by operation name params", async ({
    page,
  }) => {
    await page.goto("/search-results?operation-name=thunder&area=1015");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/case-search?*`);
    const areaSelect = await page.getByTestId("search-operation-area");
    const options = await areaSelect.locator("option").allTextContents();
    expect(options).toHaveLength(51);
    await expect(page.locator("h1")).toHaveText(
      `Search results for operation "thunder"`,
    );
    await expect(
      page.getByText(
        "2 cases found in Sussex. Select a case to view more details.",
      ),
    ).toBeVisible();

    await expect(page.getByTestId("search-operation-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-operation-area")).toHaveValue("1015");
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
    await page.goto("/search-results?defendant-name=thunder&area=1057708");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/case-search?*`);
    const areaSelect = await page.getByTestId("search-defendant-area");
    const options = await areaSelect.locator("option").allTextContents();
    expect(options).toHaveLength(51);
    await expect(page.locator("h1")).toHaveText(
      `Search results for defendant surname "thunder"`,
    );
    await expect(
      page.getByText(
        "2 cases found in Organised Crime Division. Select a case to view more details.",
      ),
    ).toBeVisible();

    await expect(page.getByTestId("search-defendant-name")).toHaveValue(
      "thunder",
    );
    await expect(page.getByTestId("search-defendant-area")).toHaveValue(
      "1057708",
    );
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
    await page.waitForResponse(`https://mocked-out-api/api/case-search?*`);
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("2 cases found. Select a case to view more details."),
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
      http.get("https://mocked-out-api/api/case-search", async () => {
        await delay(10);
        return HttpResponse.json([]);
      }),
    );
    await page.goto("/search-results?urn=11AA2222233");
    await page.waitForResponse(`https://mocked-out-api/api/case-search?*`);

    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("There are no cases matching the urn."),
    ).toBeVisible();

    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(3);
    await expect(listItems).toHaveText([
      "check for spelling mistakes in the urn.",
      "check the Case Management System to make sure the case exists and that you have access.",
      "contact the product team if you need further help.",
    ]);
  });

  test("should show validation error for search by urn in the search result page", async ({
    page,
  }) => {
    await page.goto("/search-results?urn=11AA222223312");
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-urn-link")
        .getByText("Enter a valid unique reference number"),
    ).toBeVisible();

    page.getByTestId("search-urn-link").click();
    await expect(page.getByTestId("search-urn")).toBeFocused();
    await page.getByTestId("search-urn").fill("");
    await page.locator('button:text("search")').click();
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page.getByTestId("search-urn-link").getByText("URN should not be empty"),
    ).toBeVisible();
    page.getByTestId("search-urn-link").click();
    await expect(page.getByTestId("search-urn")).toBeFocused();
  });

  test("should show validation error for search by operation name in the search result page", async ({
    page,
    worker,
  }) => {
    const emptyHomeAreaResponse = { ...caseAreasPlaywright, homeArea: {} };
    await worker.use(
      http.get("https://mocked-out-api/api/areas", async () => {
        await delay(10);
        return HttpResponse.json(emptyHomeAreaResponse);
      }),
    );
    await page.goto("/search-results?operation-name=&area=123");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-operation-name-link")
        .getByText("Operation name should not be empty"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-operation-area-link")
        .getByText("Operation area should not be empty"),
    ).toBeVisible();
    page.getByTestId("search-operation-name-link").click();
    await expect(page.getByTestId("search-operation-name")).toBeFocused();
    page.getByTestId("search-operation-area-link").click();
    await expect(page.getByTestId("search-operation-area")).toBeFocused();
    await page.getByTestId("search-operation-area").selectOption("Surrey");
    await page.getByTestId("search-operation-name").fill("abc");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?operation-name=abc&area=1001");
    await expect(page.locator("h1")).toHaveText(
      `Search results for operation  "abc"`,
    );
    await expect(
      page.getByText(
        "2 cases found in Surrey. Select a case to view more details.",
      ),
    ).toBeVisible();
  });

  test("should show validation error for search by defendant surname in the search result page", async ({
    page,
    worker,
  }) => {
    const emptyHomeAreaResponse = { ...caseAreasPlaywright, homeArea: {} };
    await worker.use(
      http.get("https://mocked-out-api/api/areas", async () => {
        await delay(10);
        return HttpResponse.json(emptyHomeAreaResponse);
      }),
    );
    await page.goto("/search-results?defendant-name=&area=234");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-defendant-name-link")
        .getByText("Defendant surname should not be empty"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-defendant-area-link")
        .getByText("Defendant area should not be empty"),
    ).toBeVisible();
    page.getByTestId("search-defendant-name-link").click();
    await expect(page.getByTestId("search-defendant-name")).toBeFocused();
    page.getByTestId("search-defendant-area-link").click();
    await expect(page.getByTestId("search-defendant-area")).toBeFocused();
    await page.getByTestId("search-defendant-area").selectOption("Surrey");
    await page.getByTestId("search-defendant-name").fill("abc");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?defendant-name=abc&area=1001");
    await expect(page.locator("h1")).toHaveText(
      `Search results for defendant surname  "abc"`,
    );
    await expect(
      page.getByText(
        "2 cases found in Surrey. Select a case to view more details.",
      ),
    ).toBeVisible();
  });

  test("Should be able to submit the form using enter button", async ({
    page,
  }) => {
    await page.goto("/search-results?urn=11AA222223312");
    await page.getByTestId("search-urn").fill("11AA2222231");
    await page.getByTestId("search-urn").press("Enter");
    await expect(page).toHaveURL("search-results?urn=11AA2222231");
  });

  test("Should show the error if the user lands on the search results page for defendant surname search and the area api failed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/areas", async () => {
        await delay(10);
        return new HttpResponse(null, { status: 500 });
      }),
    );
    await page.goto("/search-results?defendant-name=ww&area=234");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await expect(page.locator("h1")).toHaveText(`Something went wrong!`);
    await expect(
      page.getByText("Error: areas api failed with status: 500, method:GET"),
    ).toBeVisible();
  });

  test("Should show the error if the user lands on the search results page for operation name search and the area api failed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/areas", async () => {
        await delay(10);
        return new HttpResponse(null, { status: 500 });
      }),
    );
    await page.goto("/search-results?operation-name=ww&area=234");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await expect(page.locator("h1")).toHaveText(`Something went wrong!`);
    await expect(
      page.getByText("Error: areas api failed with status: 500, method:GET"),
    ).toBeVisible();
  });

  test("Should show the error if the user lands on the search results page and search api failed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/case-search", async () => {
        await delay(10);
        return new HttpResponse(null, { status: 500 });
      }),
    );
    await page.goto("/search-results?operation-name=ww&area=1001");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/case-search?*`);
    await expect(page.locator("h1")).toHaveText(`Something went wrong!`);
    await expect(
      page.getByText(
        "Error: case-search api failed with status: 500, method:GET",
      ),
    ).toBeVisible();

    await page.goto("/search-results?defendant-name=ww&area=1001");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.waitForResponse(`https://mocked-out-api/api/case-search?*`);
    await expect(page.locator("h1")).toHaveText(`Something went wrong!`);
    await expect(
      page.getByText(
        "Error: case-search api failed with status: 500, method:GET",
      ),
    ).toBeVisible();

    await page.goto("/search-results?urn=11AA2222233");
    await page.waitForResponse(`https://mocked-out-api/api/case-search?*`);
    await expect(page.locator("h1")).toHaveText(`Something went wrong!`);
    await expect(
      page.getByText(
        "Error: case-search api failed with status: 500, method:GET",
      ),
    ).toBeVisible();
  });
});
