import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";
import { Page } from "@playwright/test";

test.describe("netapp connect", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/v1/areas`);
  });

  const validateFolderPath = async (page: Page, expectedResult: string[]) => {
    const texts = await page.locator("ol>li").allTextContents();
    expect(texts).toEqual(expectedResult);
  };

  test("Should successfully connect to an netapp folder", async ({ page }) => {
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
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );

    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();

    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await expect(
      page.getByText(
        `Confirm you want to link "thunderstrike" network shared drive folder to the case?`,
      ),
    ).toBeVisible();
    await expect(
      page.getByText(`You can change the linked folder later if needed.`),
    ).toBeVisible();

    await page.getByRole("link", { name: "Back" }).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.getByTestId("radio-netapp-connect-no").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    page.getByTestId("radio-netapp-connect-yes").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL(
      "http://localhost:5173/case/14/case-management",
    );
  });

  test("Should show error page if user failed to connect to an netapp folder", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/netapp/connections",
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

    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL("/case/14/netapp-connect/error");
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL("/case/14/netapp-connect/error");
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there was a problem connecting to network Shared Drive",
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

  test("Should show no results page if the netapp folders return empty results", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/netapp/folders", async () => {
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
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      "Link a network shared drive folder to the case",
    );
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await expect(page.getByTestId("no-documents-text")).toBeVisible();
    await expect(page.getByTestId("no-documents-text")).toHaveText(
      "There are no documents currently in this folder",
    );
  });

  test("Should show error page if the netapp folders api failed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/netapp/folders", async () => {
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
        "Error: API_ERROR: An error occurred contacting the server at https://mocked-out-api/api/v1/netapp/folders: getting netapp folders failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });

  test("Should show the netapp folders results correctly", async ({ page }) => {
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
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );

    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );
    await expect(
      page.getByText(
        "If the folder you need is not listed, check that you have the correct permissions or contact the product team for support.",
      ),
    ).toBeVisible();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home"]);
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
        hasText: "Link a network shared drive folder to the case",
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
        hasText: "Are you sure?",
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
          "Sorry, there was a problem connecting to network Shared Drive",
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
    await expect(page.locator("h1")).toHaveText("Search results");
    await expect(
      page.getByText(
        "4 cases found. Select view to transfer files or folders or connect to setup storage locations.",
      ),
    ).toBeVisible();
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );

    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );

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

  test("Should correctly navigate through the folders and handle folder results and paths correctly when navigate back from the connect confirmation page", async ({
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
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Link a network shared drive folder to the case`,
    );
    await validateFolderPath(page, ["Home"]);
    await page.locator('role=button[name="thunderstrike"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "thunderstrike"]);
    await page.locator('role=button[name="folder-0"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "thunderstrike", "folder-0"]);
    await page.locator('role=button[name="thunderstrike"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "thunderstrike"]);
    await page.locator('role=button[name="Connect folder"]').nth(0).click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.getByTestId("radio-netapp-connect-no").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "thunderstrike"]);
    await page.locator('role=button[name="Connect folder"]').nth(0).click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await page.getByRole("link", { name: "Back" }).click();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "thunderstrike"]);
    await page.locator('role=button[name="folder-0"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home", "thunderstrike", "folder-0"]);
    await page.locator('role=button[name="Home"]').click();
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await validateFolderPath(page, ["Home"]);
    await expect(
      page.locator('role=button[name="thunderstrike"]'),
    ).toBeVisible();
    await expect(
      page.locator('role=button[name="thunderstrikeab"]'),
    ).toBeVisible();
  });
});
