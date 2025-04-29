import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";

test.describe("netapp connect", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
  });

  test("Should successfully connect to an netapp folder", async ({ page }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("4 cases found. Select a case to view more details."),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );

    await expect(page.getByTestId("netapp-folder-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(page.getByTestId("netapp-folder-loader")).not.toBeVisible();

    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    await expect(
      page.getByText(
        `Are you sure you want to link "thunderstrike" network shared drive folder to the case?`,
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        `Once linked, this folder will be used for transferring files related to the case. You can update the linked folder later if needed.`,
      ),
    ).toBeVisible();

    await page.getByRole("link", { name: "Back" }).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    await page.getByTestId("radio-netapp-connect-no").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    page.getByTestId("radio-netapp-connect-yes").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL("/case/14/case-overview/transfer-material");
  });

  test("Should show error page if user failed to connect to an netapp folder", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post("https://mocked-out-api/api/netapp/connections", async () => {
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
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("4 cases found. Select a case to view more details."),
    ).toBeVisible();

    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );
    await expect(page.getByTestId("netapp-folder-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(page.getByTestId("netapp-folder-loader")).not.toBeVisible();
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL("/case/14/netapp-connect/error");
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL("/case/14/netapp-connect/error");
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there was a problem connecting to network shared drive",
    );
    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toHaveText([
      "check the Case Management System to make sure the case exists and that you have access.",
      "contact the product team if you need further help.",
    ]);
    await page.getByRole("link", { name: "Search for another case" }).click();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
  });

  test("Should show no results page if the netapp folder search return empty results", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/netapp/folders", async () => {
        await delay(10);
        return HttpResponse.json({
          data: {
            rootPath: "",
            folders: [],
          },
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
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("4 cases found. Select a case to view more details."),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      "Select a network shared drive folder to link to the case",
    );
    await expect(page.getByTestId("netapp-folder-loader")).not.toBeVisible();
    await expect(
      page.getByText("There are no documents currenlty in this folder"),
    ).toBeVisible();
  });

  test("Should show error page if the netapp folders api failed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/netapp/folders", async () => {
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
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("4 cases found. Select a case to view more details."),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
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
        "Error: API_ERROR: An error occurred contacting the server at https://mocked-out-api/api/netapp/folders: getting netapp folders failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });

  test("Should show the netapp folder search results correctly", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("4 cases found. Select a case to view more details."),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );

    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );
    await expect(
      page.getByText("Select a folder from the list to link it to this case."),
    ).toBeVisible();
    await expect(
      page.getByText(
        "If the folder you need is not listed, check that you have the correct permissions or contact the product team for support.",
      ),
    ).toBeVisible();
    await expect(page.getByTestId("netapp-folder-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(page.getByTestId("netapp-folder-loader")).not.toBeVisible();
    const tableHeadValues = await page
      .locator("table thead tr:nth-child(1) th")
      .allTextContents();
    expect(tableHeadValues).toEqual([" Folder name", ""]);
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual(["thunderstrike", "Connect folder"]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual(["thunderstrikeab", "Connect folder"]);
    await expect(
      page.getByRole("button", { name: "Connect folder" }).nth(0),
    ).not.toBeDisabled();
    await expect(
      page.getByRole("button", { name: "Connect folder" }).nth(1),
    ).toBeDisabled();
  });

  test("Should navigate user back the case search page if the user lands directly on any of the netapp connect page", async ({
    page,
  }) => {
    await page.goto("/case/14/netapp-connect?operation-name=Thunderstruck3_pl");
    await expect(
      page.locator("h1", {
        hasText: "Select a network shared drive folder to link to the case",
      }),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });

  test("Should navigate user back the case search page if the user lands directly on any of the netapp connect confirmation page", async ({
    page,
  }) => {
    await page.goto("/case/14/netapp-connect/confirmation");
    await expect(
      page.locator("h1", {
        hasText: "Confirm folder link",
      }),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });

  test("Should navigate user back the case search page if the user lands directly on any of the netapp connect error page", async ({
    page,
  }) => {
    await page.goto("/case/14/netapp-connect/error");
    await expect(
      page.locator("h1", {
        hasText:
          "Sorry, there was a problem connecting to network shared drive",
      }),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });
  test("Should work correctly all the back link navigation until netapp connect", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("4 cases found. Select a case to view more details."),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );

    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select a network shared drive folder to link to the case`,
    );

    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("4 cases found. Select a case to view more details."),
    ).toBeVisible();
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    await expect(page.getByTestId("search-urn")).toHaveValue("11AA2222233");
  });
});
