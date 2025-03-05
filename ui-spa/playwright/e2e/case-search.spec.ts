import { test, expect } from "@playwright/test";

test("Hello World Test", async ({ page }) => {
  await page.goto("/");

  await expect(page.locator("h1")).toHaveText("Find a case");
  await expect(page.getByRole("button", { name: "Search" })).toBeVisible();
});
