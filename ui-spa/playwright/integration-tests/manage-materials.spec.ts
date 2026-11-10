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

  test("Should enable the actions on selection options only when materials are selected", async ({
    page,
  }) => {
    const selectionActions = [
      "Copy",
      "Move",
      "Rename selected items",
      "Delete",
    ];
    await goToCaseManagement(page, "case-details=true");
    await page.getByTestId("tab-1").click();
    await expect(page.getByTestId("materials-table")).toBeVisible();

    const actionsButton = page.getByRole("button", {
      name: "Actions on selection",
    });
    const actionsPanel = page.getByTestId("dropdown-panel");

    await actionsButton.click();
    for (const name of selectionActions) {
      await expect(actionsPanel.getByRole("button", { name })).toBeDisabled();
    }
    await actionsButton.click();

    await page.getByTestId("material-checkbox-file-1-0.pdf").check();
    await actionsButton.click();
    await expect(actionsPanel.getByRole("button")).toHaveText(selectionActions);
    for (const name of selectionActions) {
      await expect(actionsPanel.getByRole("button", { name })).toBeEnabled();
    }
  });

  test("Should not show the manage materials tab if the feature is turned off", async ({
    page,
  }) => {
    await goToCaseManagement(page, "case-details=false");
    await expect(page.getByTestId("tab-1")).toHaveText("Activity log");
    await expect(page.getByTestId("tab-2")).not.toBeVisible();
  });
});
