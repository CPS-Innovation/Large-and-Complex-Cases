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

  test("Should show the transfer error page, if the transfer status is `PartiallyCompleted` with mix of fileExists error and other errors", async ({
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
            failedItems: [
              {
                sourcePath: "folder1/file1.txt",
                errorCode: "FileExists",
              },
              {
                sourcePath: "folder1/file2.txt",
                errorCode: "FileExists",
              },
              {
                sourcePath: "folder1/file3.txt",
                errorCode: "GeneralError",
              },
            ],
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

    await expect(page.getByTestId("file-exists-error-wrapper")).toBeVisible();
    await expect(
      page.getByTestId("file-exists-error-wrapper").locator("h2"),
    ).toHaveText("Some files already exist in the destination folder");
    await expect(
      page.getByTestId("already-exist-files-list"),
    ).not.toBeVisible();
    await expect(
      page.getByTestId("file-exists-error-wrapper").locator("details>summary"),
    ).toHaveText("View files");
    await page
      .getByTestId("file-exists-error-wrapper")
      .locator("details>summary")
      .click();
    await expect(page.getByTestId("already-exist-files-list")).toBeVisible();
    const listItems = page
      .getByTestId("already-exist-files-list")
      .locator("li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toContainText([
      "folder1/file1.txt",
      "folder1/file2.txt",
    ]);

    await expect(page.getByTestId("other-failed-error-wrapper")).toBeVisible();
    await expect(
      page.getByTestId("other-failed-error-wrapper").locator("h2"),
    ).toHaveText("Some files could not be transferred");
    await expect(page.getByTestId("other-failed-files-list")).not.toBeVisible();
    await expect(
      page.getByTestId("other-failed-error-wrapper").locator("details>summary"),
    ).toHaveText("View files");
    await page
      .getByTestId("other-failed-error-wrapper")
      .locator("details>summary")
      .click();
    await expect(page.getByTestId("other-failed-files-list")).toBeVisible();
    const listItems1 = page
      .getByTestId("other-failed-files-list")
      .locator("li");
    await expect(listItems1).toHaveCount(1);
    await expect(listItems1).toContainText(["folder1/file3.txt"]);
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/case/12/case-management",
    );

    await expect(page.getByTestId("user-actions-wrapper")).toBeVisible();
    await expect(
      page.getByTestId("user-actions-wrapper").locator("h2"),
    ).toHaveText("What you can do");
    const listItems2 = page
      .getByTestId("user-actions-wrapper")
      .locator("ul > li");
    await expect(listItems2).toHaveCount(3);
    await expect(listItems2).toHaveText([
      "remove or rename any duplicate files, then try again",
      "check the activity log to see if any files transferred successfully",
      "contact the product team for help and include the error message failed transfer - transfer-id-egress-to-netapp",
    ]);

    await expect(page.getByRole("button", { name: "Continue" })).toBeVisible();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
  });
  test("Should show the transfer error page, if the transfer status is `Failed` with no fileExists error", async ({
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
            failedItems: [
              {
                sourcePath: "folder1/file1.txt",
                errorCode: "GeneralError",
              },
              {
                sourcePath: "folder1/file2.txt",
                errorCode: "GeneralError",
              },
              {
                sourcePath: "folder1/file3.txt",
                errorCode: "GeneralError",
              },
            ],
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
    await expect(
      page.getByTestId("file-exists-error-wrapper"),
    ).not.toBeVisible();
    await expect(page.getByTestId("other-failed-error-wrapper")).toBeVisible();
    await expect(
      page.getByTestId("other-failed-error-wrapper").locator("h2"),
    ).toHaveText("Some files could not be transferred");
    await expect(page.getByTestId("other-failed-files-list")).not.toBeVisible();
    await expect(
      page.getByTestId("other-failed-error-wrapper").locator("details>summary"),
    ).toHaveText("View files");
    await page
      .getByTestId("other-failed-error-wrapper")
      .locator("details>summary")
      .click();
    await expect(page.getByTestId("other-failed-files-list")).toBeVisible();
    const listItems1 = page
      .getByTestId("other-failed-files-list")
      .locator("li");
    await expect(listItems1).toHaveCount(3);
    await expect(listItems1).toContainText([
      "folder1/file1.txt",
      "folder1/file2.txt",
      "folder1/file3.txt",
    ]);
    await expect(page.getByTestId("user-actions-wrapper")).toBeVisible();
    await expect(
      page.getByTestId("user-actions-wrapper").locator("h2"),
    ).toHaveText("What you can do");
    const listItems2 = page
      .getByTestId("user-actions-wrapper")
      .locator("ul > li");
    await expect(listItems2).toHaveCount(3);
    await expect(listItems2).toHaveText([
      "try again",
      "check the activity log to see if any files transferred successfully",
      "contact the product team for help and include the error message failed transfer - transfer-id-egress-to-netapp",
    ]);
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/case/12/case-management",
    );
    await expect(page.getByRole("button", { name: "Continue" })).toBeVisible();
    await page.getByRole("link", { name: "Back" }).click();
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
  });
  test("Should show the transfer error page, if the transfer status is `Failed` with only fileExists error", async ({
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
            failedItems: [
              {
                sourcePath: "folder1/file1.txt",
                errorCode: "FileExists",
              },
              {
                sourcePath: "folder1/file2.txt",
                errorCode: "FileExists",
              },
              {
                sourcePath: "folder1/file3.txt",
                errorCode: "FileExists",
              },
            ],
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

    await expect(
      page.getByTestId("other-failed-error-wrapper"),
    ).not.toBeVisible();
    await expect(page.getByTestId("file-exists-error-wrapper")).toBeVisible();
    await expect(
      page.getByTestId("file-exists-error-wrapper").locator("h2"),
    ).toHaveText("Some files already exist in the destination folder");
    await expect(
      page.getByTestId("already-exist-files-list"),
    ).not.toBeVisible();
    await expect(
      page.getByTestId("file-exists-error-wrapper").locator("details>summary"),
    ).toHaveText("View files");
    await page
      .getByTestId("file-exists-error-wrapper")
      .locator("details>summary")
      .click();
    await expect(page.getByTestId("already-exist-files-list")).toBeVisible();
    const listItems = page
      .getByTestId("already-exist-files-list")
      .locator("li");
    await expect(listItems).toHaveCount(3);
    await expect(listItems).toContainText([
      "folder1/file1.txt",
      "folder1/file2.txt",
      "folder1/file3.txt",
    ]);
    await expect(page.getByTestId("user-actions-wrapper")).toBeVisible();
    await expect(
      page.getByTestId("user-actions-wrapper").locator("h2"),
    ).toHaveText("What you can do");
    const listItems2 = page
      .getByTestId("user-actions-wrapper")
      .locator("ul > li");
    await expect(listItems2).toHaveCount(3);

    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/case/12/case-management",
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
