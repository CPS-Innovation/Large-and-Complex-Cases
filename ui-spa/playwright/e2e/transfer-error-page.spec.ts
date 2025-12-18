import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
import { TransferStatusResponse } from "../../src/common/types/TransferStatusResponse";
import { Page } from "@playwright/test";

test.describe("transfer-error-page", () => {
  const startTransfer = async (page: Page) => {
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer materials to the Shared Drive");
    await page
      .getByTestId("egress-table-wrapper")
      .locator('role=button[name="folder-1-0"]')
      .click();
    const checkboxes = page
      .getByTestId("egress-table-wrapper")
      .locator('input[type="checkbox"]');
    await checkboxes.nth(0).check();
    await expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    await expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
    await expect(page.getByTestId("netapp-inset-text")).toBeVisible();
    await page
      .getByTestId("netapp-inset-text")
      .getByRole("button", { name: "Copy" })
      .click();
    const confirmationModal = await page.getByTestId("div-modal");
    await expect(confirmationModal).toBeVisible();
    await expect(confirmationModal).toContainText("Confirm");
    await expect(
      confirmationModal.getByLabel(
        "I want to copy 2 folders and 1 file to netapp",
      ),
    ).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).toBeDisabled();
    await confirmationModal
      .getByLabel("I want to copy 2 folders and 1 file to netapp")
      .click();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).not.toBeDisabled();
    await confirmationModal.getByRole("button", { name: "Continue" }).click();
    await expect(confirmationModal).not.toBeVisible();
  };

  test("Should show the transfer error page, if the transfer status is `PartiallyCompleted`", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "PartiallyCompleted",
            transferType: "Copy",
            direction: "EgressToNetApp",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
          } as TransferStatusResponse);
        },
      ),
    );
    await startTransfer(page);
    await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer materials to the Shared Drive",
      }),
    ).not.toBeVisible();
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Indexing transfer from Egress to Shared Drive...");
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Completing transfer from Egress to Shared Drive...");
    await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
    await expect(page).toHaveURL("/case/12/case-management/transfer-errors");
    await expect(page.locator("h1")).toHaveText(
      "There is a problem transferring files",
    );
    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toHaveText([
      "try again",
      "check the activity log to see if any files or folders have transferred successfully",
    ]);
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/case/12/case-management",
    );
    await expect(page.getByTestId("contact-information")).toHaveText(
      "To get help, call the Service Desk 0800 692 6996. Tell them you're seeing error code: transfer-id-egress-to-netapp.",
    );
    await expect(page.getByRole("button", { name: "Continue" })).toBeVisible();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
  });
  test("Should show the transfer error page, if the transfer status is `Failed`", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "Failed",
            transferType: "Copy",
            direction: "EgressToNetApp",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
          } as TransferStatusResponse);
        },
      ),
    );
    await startTransfer(page);
    await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer materials to the Shared Drive",
      }),
    ).not.toBeVisible();
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Indexing transfer from Egress to Shared Drive...");
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Completing transfer from Egress to Shared Drive...");
    await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
    await expect(page).toHaveURL("/case/12/case-management/transfer-errors");
    await expect(page.locator("h1")).toHaveText(
      "There is a problem transferring files",
    );
    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toHaveText([
      "try again",
      "check the activity log to see if any files or folders have transferred successfully",
    ]);
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/case/12/case-management",
    );
    await expect(page.getByTestId("contact-information")).toHaveText(
      "To get help, call the Service Desk 0800 692 6996. Tell them you're seeing error code: transfer-id-egress-to-netapp.",
    );
    await expect(page.getByRole("button", { name: "Continue" })).toBeVisible();
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
  });
  test("User should not be able to land directly on the transfer error page,it should be redirected to search case page", async ({
    page,
  }) => {
    await page.goto("/case/12/case-management/transfer-errors");
    await expect(page).toHaveURL("/");
  });
});
