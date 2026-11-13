import { delay, HttpResponse, http } from "msw";
import { expect, test } from "./utils/test";
import { Page } from "@playwright/test";

test.describe("netapp connect", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/v1/areas`);
  });

  const validateFolderPath = async (page: Page, expectedResult: string[]) => {
    await expect(page.locator("ol>li")).toHaveText(expectedResult, {
      timeout: 50000,
    });
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
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Connect a Shared Drive folder to this case`,
    );

    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();

    await page.getByRole("button", { name: "Connect" }).first().click();

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
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Connect a Shared Drive folder to this case`,
    );
    await expect(page.getByTestId("netapp-folder-table-loader")).toBeVisible();
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/14/connect-shared-drive-folder-error");
    await page.getByRole("button", { name: "Try again" }).click();
    await expect(page).toHaveURL(
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Connect a Shared Drive folder to this case`,
    );
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/14/connect-shared-drive-folder-error");
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to the Shared Drive",
    );
    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toHaveText([
      "return to the previous screen and try to connect again",
      "contact the product team if you need help",
    ]);
    await page.getByRole("button", { name: "Try again" }).click();
    await expect(page).toHaveURL(
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      "Connect a Shared Drive folder to this case",
    );
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
            maxKeys: 100,
            nextContinuationToken: null,
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
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      "Connect a Shared Drive folder to this case",
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
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText("Contact the product team and give them the error code."),
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
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );

    await expect(page.locator("h1")).toHaveText(
      `Connect a Shared Drive folder to this case`,
    );
    await expect(
      page.getByText("Select a folder to connect it to this case."),
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
    expect(tableHeadValues).toEqual(["Action", " Folder name"]);
    const row1Values = await page
      .locator("table tbody tr:nth-child(1) td")
      .allTextContents();
    expect(row1Values).toEqual(["Connect", "thunderstrike"]);
    const row2Values = await page
      .locator("table tbody tr:nth-child(2) td")
      .allTextContents();
    expect(row2Values).toEqual(["", "thunderstrikeab"]);
    await expect(
      page.getByRole("button", { name: "Connect" }).nth(0),
    ).not.toBeDisabled();
  });

  test("Should navigate user back the case search page if the user lands directly on any of the netapp connect page", async ({
    page,
  }) => {
    await page.goto(
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(
      page.locator("h1", {
        hasText: "Connect a Shared Drive folder to this case",
      }),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });

  test("Should navigate user back the case search page if the user lands directly on any of the netapp connect error page", async ({
    page,
  }) => {
    await page.goto("/case/14/connect-shared-drive-folder-error");
    await expect(
      page.locator("h1", {
        hasText: "There is a problem connecting to the Shared Drive",
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
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Connect a Shared Drive folder to this case`,
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

  test("Should correctly navigate through the folders and handle folder results and paths correctly", async ({
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
      "/case/14/shared-drive-connect?operation-name=Thunderstruck3_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Connect a Shared Drive folder to this case`,
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
