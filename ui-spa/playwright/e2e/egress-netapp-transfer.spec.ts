import { expect, test } from "../utils/test";

test.describe("egress-netapp-transfer", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
  });
  test("Should show and hide the actions button and current folder text indent in the netapp side based on the files and folders selected on the egress side ", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer folders and files between egress and shared drive");

    const checkboxes = page
      .getByTestId("egress-table-wrapper")
      .locator('input[type="checkbox"]');
    await checkboxes.nth(0).check();
    const count = await checkboxes.count();
    for (let i = 0; i < count; i++) {
      await expect(checkboxes.nth(i)).toBeChecked();
    }
    await checkboxes.nth(0).uncheck();
    const newCount = await checkboxes.count();
    for (let i = 0; i < newCount; i++) {
      await expect(checkboxes.nth(i)).not.toBeChecked();
    }
    await checkboxes.nth(0).check();
    expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toHaveText(
      "Transfer to netappCopy |Move",
    );
    await checkboxes.nth(0).uncheck();
    expect(page.getByTestId("transfer-actions-dropdown-0")).not.toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).not.toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).not.toBeVisible();

    await checkboxes.nth(1).check();
    expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toHaveText(
      "Transfer to netappCopy |Move",
    );
    await checkboxes.nth(1).uncheck();
    expect(page.getByTestId("transfer-actions-dropdown-0")).not.toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).not.toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).not.toBeVisible();

    await checkboxes.nth(3).check();
    expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toHaveText(
      "Transfer to netappCopy |Move",
    );
    await checkboxes.nth(3).uncheck();
    expect(page.getByTestId("transfer-actions-dropdown-0")).not.toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).not.toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).not.toBeVisible();
  });
  test("Should show the transfer confirmation pop up with correct texts when user chooses copy operation from inset text", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer folders and files between egress and shared drive");
    const checkboxes = page
      .getByTestId("egress-table-wrapper")
      .locator('input[type="checkbox"]');
    await checkboxes.nth(0).check();
    expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toBeVisible();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Copy" })
      .click();
    const confirmationModal = await page.getByTestId("div-modal");
    await expect(confirmationModal).toBeVisible();
    await expect(confirmationModal).toContainText("Copy files to: netapp");
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to copy 2 folders and 1 file to netapp",
      ),
    ).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Cancel" }),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await expect(confirmationModal).not.toBeVisible();
    await checkboxes.nth(0).uncheck();
    await checkboxes.nth(1).check();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Copy" })
      .click();
    await expect(confirmationModal).toBeVisible();
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to copy 1 folder to netapp",
      ),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await checkboxes.nth(2).check();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Copy" })
      .click();
    await expect(confirmationModal).toBeVisible();
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to copy 2 folders to netapp",
      ),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await expect(confirmationModal).not.toBeVisible();
  });
  test("Should show the transfer confirmation pop up with correct texts when user chooses move operation from inset text", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer folders and files between egress and shared drive");
    const checkboxes = page
      .getByTestId("egress-table-wrapper")
      .locator('input[type="checkbox"]');
    await checkboxes.nth(0).check();
    expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toBeVisible();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Move" })
      .click();
    const confirmationModal = await page.getByTestId("div-modal");
    await expect(confirmationModal).toBeVisible();
    await expect(confirmationModal).toContainText("Move files to: netapp");
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to move 2 folders and 1 file to netapp",
      ),
    ).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Cancel" }),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await expect(confirmationModal).not.toBeVisible();
    await checkboxes.nth(0).uncheck();
    await checkboxes.nth(1).check();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Move" })
      .click();
    await expect(confirmationModal).toBeVisible();
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to move 1 folder to netapp",
      ),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await checkboxes.nth(2).check();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Move" })
      .click();
    await expect(confirmationModal).toBeVisible();
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to move 2 folders to netapp",
      ),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await expect(confirmationModal).not.toBeVisible();
    await page
      .getByTestId("netapp-table-wrapper")
      .getByRole("button", { name: "folder-1-1" })
      .click();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Move" })
      .click();
    await expect(confirmationModal).toBeVisible();
    await expect(confirmationModal).toContainText("Move files to: folder-1-1");
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to move 2 folders to folder-1-1",
      ),
    ).toBeVisible();
  });
  test("Should show the transfer confirmation pop up with correct texts when user chooses copy/move operation from the action dropdown", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer folders and files between egress and shared drive");
    const checkboxes = page
      .getByTestId("egress-table-wrapper")
      .locator('input[type="checkbox"]');
    await checkboxes.nth(0).check();
    expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
    expect(page.getByTestId("netapp-inset-text")).toBeVisible();
    await page.getByTestId("transfer-actions-dropdown-0").click();
    await expect(page.getByTestId("dropdown-panel")).toBeVisible();
    await page.getByTestId("transfer-actions-dropdown-0").click();
    await expect(page.getByTestId("dropdown-panel")).not.toBeVisible();
    await page.getByTestId("transfer-actions-dropdown-0").click();
    await page
      .getByTestId("dropdown-panel")
      .getByRole("button", { name: "Move" })
      .click();
    const confirmationModal = await page.getByTestId("div-modal");
    await expect(confirmationModal).toBeVisible();
    await expect(confirmationModal).toContainText("Move files to: folder-1-0");
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to move 2 folders and 1 file to folder-1-0",
      ),
    ).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Cancel" }),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await page.getByTestId("transfer-actions-dropdown-1").click();
    await page
      .getByTestId("dropdown-panel")
      .getByRole("button", { name: "Copy" })
      .click();
    await expect(confirmationModal).toBeVisible();
    await expect(confirmationModal).toContainText("Copy files to: folder-1-1");
    await expect(
      confirmationModal.getByLabel(
        "I confirm I want to copy 2 folders and 1 file to folder-1-1",
      ),
    ).toBeVisible();
    await confirmationModal.getByRole("button", { name: "Cancel" }).click();
    await page
      .getByTestId("netapp-table-wrapper")
      .getByRole("button", { name: "folder-1-1" })
      .click();
    await page.getByTestId("transfer-actions-dropdown-0").click();
    await page
      .getByTestId("dropdown-panel")
      .getByRole("button", { name: "Move" })
      .click();
    const newConfirmationModal = await page.getByTestId("div-modal");
    await expect(newConfirmationModal).toBeVisible();
    await expect(newConfirmationModal).toContainText(
      "Move files to: folder-2-0",
    );
    await expect(
      newConfirmationModal.getByLabel(
        "I confirm I want to move 2 folders and 1 file to folder-2-0",
      ),
    ).toBeVisible();
  });
});
