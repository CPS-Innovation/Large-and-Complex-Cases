import { expect, test } from "./utils/test";
import { Page } from "@playwright/test";

test.describe("Manage materials", () => {
  const goToCaseManagement = async (page: Page, searchParam: string = "") => {
    await page.goto(`/case/12/case-management?${searchParam}`);
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);

    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
  };

  test("Should show the manage materials tab correctly", async ({ page }) => {
    await goToCaseManagement(page, "case-details=true");
    await page.getByTestId("tab-1").click();
    await expect(page.getByTestId("tab-active")).toHaveText("Manage materials");
    await expect(page.getByTestId("tab-2")).toHaveText("Activity log");
  });

  test("Should not show the manage materials tab if the feature is turned off", async ({
    page,
  }) => {
    await goToCaseManagement(page, "case-details=false");
    await expect(page.getByTestId("tab-1")).toHaveText("Activity log");
    await expect(page.getByTestId("tab-2")).not.toBeVisible();
  });
});
