import { delay, HttpResponse, http } from "msw";
import { expect, test } from "./utils/test";

test.describe("Skip-link test ", () => {
  test("Case Search page - clicking skip-link should take you to the main content", async ({
    page,
  }) => {
    await page.goto("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
    await delay(500);
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("radio", { name: "URN (Unique Reference Number)" }),
    ).toBeFocused();
  });
  test("Case Search Results page - search by Operation Name - clicking skip-link should take you to the main content", async ({
    page,
  }) => {
    await page.goto("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
    await page.getByRole("radio", { name: "Operation name" }).check();
    await page.getByTestId("search-operation-name").fill("OperationA");
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page).toHaveURL(
      /\/search-results\?operation-name=OperationA&area=1057708/,
    );
    await delay(500);
    await expect(page.locator("h1")).toHaveText(`Search results`);
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(await page.getByTestId("search-operation-name")).toBeFocused();
  });
  test("Case Search Results page - search by defendant name - clicking skip-link should take you to the main content", async ({
    page,
  }) => {
    await page.goto("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
    await page.getByRole("radio", { name: "Defendant last name" }).check();
    await page.getByTestId("search-defendant-name").fill("defendantA");
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page).toHaveURL(
      /\/search-results\?defendant-name=defendantA&area=1057708/,
    );
    await delay(500);
    await expect(page.locator("h1")).toHaveText(`Search results`);
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(await page.getByTestId("search-defendant-name")).toBeFocused();
  });
  test("Case Search Results page - search by urn - clicking skip-link should take you to the main content", async ({
    page,
  }) => {
    await page.goto("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
    await page.getByTestId("search-urn").fill("11AA2222233");
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page).toHaveURL(/\/search-results\?urn=11AA2222233/);
    await delay(500);
    await expect(page.locator("h1")).toHaveText(`Search results`);
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(await page.getByTestId("search-urn")).toBeFocused();
  });

  test("Egress Connect, confirmation and error page - clicking skip-link should take you to the main content", async ({
    page,
    worker,
  }) => {
    await page.goto("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
    await page.getByTestId("search-urn").fill("11AA2222233");
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page).toHaveURL(/\/search-results\?urn=11AA2222233/);
    await expect(page.locator("h1")).toHaveText(`Search results`);
    await page.locator('role=link[name="Connect"]').nth(1).click();

    await expect(page).toHaveURL(
      "/case/13/egress-connect?workspace-name=Thunderstruck2_pl",
    );
    await delay(500);
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(await page.getByTestId("search-folder-name")).toBeFocused();
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/13/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await expect(
      page.getByText(
        `Confirm you want to link "thunderstrike" on Egress to the case?`,
      ),
    ).toBeVisible();
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      await page.getByTestId("radio-egress-connect-yes"),
    ).toBeFocused();
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/egress/connections",
        async () => {
          await delay(10);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page).toHaveURL("/case/13/egress-connect/error");
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to Egress",
    );
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("link", { name: /Search for another case./i }),
    ).toBeFocused();
  });

  test("Shared Drive Connect, confirmation and error page - clicking skip-link should take you to the main content", async ({
    page,
    worker,
  }) => {
    await page.goto("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
    await page.getByTestId("search-urn").fill("11AA2222233");
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page).toHaveURL(/\/search-results\?urn=11AA2222233/);
    await expect(page.locator("h1")).toHaveText(`Search results`);
    await page.locator('role=link[name="Connect"]').nth(2).click();

    await expect(page).toHaveURL(
      "/case/14/netapp-connect?operation-name=Thunderstruck3_pl",
    );
    await delay(500);
    await expect(
      page.getByText(`Loading folders from Network Shared Drive`),
    ).toBeVisible();
    await expect(
      page.getByTestId("netapp-folder-table-loader"),
    ).not.toBeVisible();
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("button", { name: /Folder name/i }),
    ).toBeFocused();
    await page.getByRole("button", { name: "Connect" }).first().click();
    await expect(page).toHaveURL("/case/14/netapp-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Are you sure?`);
    await expect(
      page.getByText(
        `Confirm you want to link "thunderstrike" Shared Drive folder to the case?`,
      ),
    ).toBeVisible();
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      await page.getByTestId("radio-netapp-connect-yes"),
    ).toBeFocused();
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/netapp/connections",
        async () => {
          await delay(10);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page).toHaveURL("/case/14/netapp-connect/error");
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to the Shared Drive",
    );
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("link", { name: /Search for another case./i }),
    ).toBeFocused();
  });

  test("Case Management Page - clicking skip-link should take you to the main content", async ({
    page,
  }) => {
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText("Thunderstruck");
    await delay(500);
    await expect(page.getByTestId("case-urn")).toHaveText("45AA2098221");
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(page.getByTestId("tab-active")).toBeFocused();
  });
});
