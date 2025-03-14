import { test, expect } from "@playwright/test";

test("Search results page", async ({ page }) => {
  await page.goto("/search-results?urn=11AA2222233");

  await expect(page.locator("h1")).toHaveText("Search for urn search");
  await expect(page.locator("body")).toContainText("Thunderstruck1_pl");
});
