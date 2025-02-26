import { test, expect } from "@playwright/test";

test("Hello World Test", async ({ page }) => {
  await page.goto("/");

  const h1 = page.locator("h1");

  await expect(h1).toHaveText("abc");
  await page.locator("button", { hasText: "click" }).click();
  await expect(h1).toHaveText("Hello World!");
});
