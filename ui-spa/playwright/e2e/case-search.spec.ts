import { test, expect } from "@playwright/test";

test.describe("Case Search", () => {
  test("case search by operation name and area", async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
    await page.getByRole("radio", { name: "Operation name" }).check();
    const input = await page.locator('label:text("Operation name") + input');
    await input.isVisible();
    await input.fill("thunder");
    const areaSelect = await page.getByTestId("search-operation-area");
    const options = await areaSelect.locator("option").allTextContents();
    await expect(options).toEqual([
      "-- Please select --",
      "SEOCID Int London and SE Div",
      "Special Crime Division",
      "Bedfordshire",
      "Cambridgeshire",
      "Cheshire",
    ]);
    areaSelect.selectOption("Cambridgeshire");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL(
      /\/search-results\?operation-name=thunder&area=4/,
    );
    await await expect(page.locator("h1")).toHaveText(
      "Search for Operation name search",
    );
  });
});
