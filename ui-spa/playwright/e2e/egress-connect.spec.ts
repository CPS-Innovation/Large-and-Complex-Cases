import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";

test.describe("egress connect", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/v1/areas`);
  });

  test("Should successfully connect to an egress folder", async ({ page }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);

    await expect(page.getByTestId("search-folder-name")).toHaveValue(
      "Thunderstruck2_pl",
    );
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/13/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await expect(
      page.getByText(
        `Confirm you want to link "thunderstrike" on Egress to the case?`,
      ),
    ).toBeVisible();

    await page.getByRole("link", { name: "Back" }).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);
    await page.getByRole("button", { name: "Connect" }).first().click();
    await page.getByTestId("radio-egress-connect-no").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/13/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    page.getByTestId("radio-egress-connect-yes").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL(
      "http://localhost:5173/case/13/case-management",
    );
  });

  test("Should show error page if user failed to connect to an egress folder", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/egress/connections",
        async () => {
          await delay(10);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/13/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL("/case/13/egress-connect/error");
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/13/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL("/case/13/egress-connect/error");
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to Egress",
    );
    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toHaveText([
      "check the case exists and you have access on the Case Management System",
      "contact the product team if you need help",
    ]);
    await page.getByRole("link", { name: "Search for another case" }).click();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
  });

  test("Should validate the egress folder name search input", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);
    await expect(page.getByTestId("search-folder-name")).toHaveValue(
      "Thunderstruck2_pl",
    );
    page.getByTestId("search-folder-name").fill("");
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page.getByTestId("search-error-summary")).toBeVisible();
    await expect(
      page.getByTestId("search-error-summary").getByText("There is a problem"),
    ).toBeVisible();
    await expect(
      page
        .getByTestId("search-folder-name-link")
        .getByText("egress folder name should not be empty"),
    ).toBeVisible();

    page.getByTestId("search-folder-name-link").click();
    await expect(page.getByTestId("search-folder-name")).toBeFocused();
    page.getByTestId("search-folder-name").fill("abc");
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page).toHaveURL("/case/13/egress-connect?workspace-name=abc");
    await expect(
      page.getByText("There are 2 cases matching abc."),
    ).toBeVisible();
  });

  test("Should show no results page if the egress folder search return empty results", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/egress/workspaces", async () => {
        await delay(10);
        return HttpResponse.json({
          data: [],
          pagination: {
            totalResults: 50,
            skip: 0,
            take: 50,
            count: 25,
          },
        });
      }),
    );
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText("Link Egress to the case");
    await expect(
      page.getByText("There are no cases matching Thunderstruck2_pl."),
    ).toBeVisible();

    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(3);
    await expect(listItems).toHaveText([
      "check for spelling or typing errors",
      "check the case exists on Egress and you have the correct permissions",
      "contact the product team if you need help",
    ]);
  });

  test("Should show error page if the egress folder search api failed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/egress/workspaces", async () => {
        await delay(10);
        return new HttpResponse(null, { status: 500 });
      }),
    );
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: API_ERROR: An error occurred contacting the server at https://mocked-out-api/api/v1/egress/workspaces: Searching for Egress workspaces failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });

  test("Should show the egress folder search results correctly", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );

    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);
    await expect(
      page.getByText(
        "If the case you need is not listed, check you have the correct permissions in Egress or contact the product team for support.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText("There are 2 cases matching Thunderstruck2_pl."),
    ).toBeVisible();
    await expect(page.getByTestId("search-folder-name")).toHaveValue(
      "Thunderstruck2_pl",
    );
    const tableHeadValues = await page
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual([
      " Operation or defendant last name",
      " Status",
      " Date created",
      "",
    ]);
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual([
      "thunderstrike",
      "Not connected",
      "03/01/2000",
      "Connect",
    ]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual([
      "thunderstrike1",
      "Connected",
      "02/01/2000",
      "Connect",
    ]);
    await expect(
      page.getByRole("button", { name: "Connect" }).nth(0),
    ).not.toBeDisabled();
    await expect(
      page.getByRole("button", { name: "Connect" }).nth(1),
    ).toBeDisabled();
  });

  test("Should navigate user back the case search page if the user lands directly on any of the egress connect page", async ({
    page,
  }) => {
    await page.goto("/case/13/egress-connect?workspace-name=Thunderstruck2_pl");
    await expect(
      page.locator("h1", {
        hasText: "Link Egress to the case",
      }),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });

  test("Should navigate user back the case search page if the user lands directly on any of the egress connect confirmation page", async ({
    page,
  }) => {
    await page.goto("/case/13/egress-connect/confirmation");
    await expect(
      page.locator("h1", {
        hasText: "Are you sure?",
      }),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });

  test("Should navigate user back the case search page if the user lands directly on any of the egress connect error page", async ({
    page,
  }) => {
    await page.goto("/case/13/egress-connect/error");
    await expect(
      page.locator("h1", {
        hasText: "There is a problem connecting to the Shared Drive",
      }),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });

  test("Should work correctly all the back link navigation until egress connect", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);

    await expect(page.getByTestId("search-folder-name")).toHaveValue(
      "Thunderstruck2_pl",
    );
    await expect(
      page.getByText("There are 2 cases matching Thunderstruck2_pl."),
    ).toBeVisible();
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/13/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await expect(page.locator("h1")).toHaveText(`Link Egress to the case`);

    await expect(page.getByTestId("search-folder-name")).toHaveValue(
      "Thunderstruck2_pl",
    );
    await expect(
      page.getByText("There are 2 cases matching Thunderstruck2_pl."),
    ).toBeVisible();
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    await expect(page.getByTestId("search-urn")).toHaveValue("11AA2222233");
  });
});
