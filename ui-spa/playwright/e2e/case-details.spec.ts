import { expect, test } from "../utils/test";
import { Page } from "@playwright/test";

test.describe("Case Details", () => {
  const goToCaseManagement = async (page: Page, searchParam: string = "") => {
    await page.goto(`/case/12/case-management?${searchParam}`);
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);

    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
  };

  test("Should show the case details tab correctly", async ({ page }) => {
    await goToCaseManagement(page);
    await page.getByTestId("tab-2").click();
    await expect(page.getByTestId("tab-active")).toHaveText("Case Details");
    await expect(page.locator("h2")).toHaveText(`Case Details`);
  });

  test("Should not show the case details tab if the feature is turned off", async ({
    page,
  }) => {
    await goToCaseManagement(page, "case-details=false");
    await expect(page.getByTestId("tab-2")).not.toBeVisible();
  });
});
