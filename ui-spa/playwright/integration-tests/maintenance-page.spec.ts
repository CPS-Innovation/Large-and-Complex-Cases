import { expect, test } from "./utils/test";
import { Page } from "@playwright/test";

test.describe("Maintenance Page", () => {
  const message1 =
    "We're carrying out planned maintenance. The service will be unavailable until 5.30pm on Tuesday 4th of August.";
  const message2 = "Please try again after 5.30pm.";

  const verifyPageElements = async (page: Page) => {
    await expect(page).toHaveURL("http://localhost:5173/maintenance");
    await expect(page.locator("h1")).toHaveText(
      "Sorry this service is unavailable",
    );
    await expect(page.locator("p").nth(0)).toHaveText(message1);
    await expect(page.locator("p").nth(1)).toHaveText(message2);
    await expect(page.locator("p").nth(2)).toHaveText(
      "If you need help, contact the product team in the Teams channel.",
    );
  };

  test("Should show maintenance page if you navigate to case management page", async ({
    page,
  }) => {
    await page.goto(`/case/12/case-management?maintenance-mode=true`);
    await verifyPageElements(page);
  });

  test("Should not show the maintenance page if the feature is turned off", async ({
    page,
  }) => {
    await page.goto(`/case/12/case-management?maintenance-mode=false`);
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
  });
  test("Should show maintenance page if you navigate to home page", async ({
    page,
  }) => {
    await page.goto(`/?maintenance-mode=true`);
    await verifyPageElements(page);
  });

  test("Should show the home page if the feature is turned off", async ({
    page,
  }) => {
    await page.goto(`/?maintenance-mode=false`);
    await expect(page).toHaveURL(
      "http://localhost:5173?maintenance-mode=false",
    );
    await expect(
      page.getByRole("radio", { name: "Operation name" }),
    ).toBeVisible();
  });

  test("Should show maintenance page if you navigate to search results page", async ({
    page,
  }) => {
    await page.goto("/search-results?urn=11AA2222233&maintenance-mode=true");
    await verifyPageElements(page);
  });

  test("Should show the search results page if the feature is turned off", async ({
    page,
  }) => {
    await page.goto(`/search-results?urn=11AA2222233&maintenance-mode=false`);
    await expect(page).toHaveURL(
      "http://localhost:5173/search-results?urn=11AA2222233&maintenance-mode=false",
    );
    await expect(page.locator("h1")).toHaveText("Search results");
  });
});
