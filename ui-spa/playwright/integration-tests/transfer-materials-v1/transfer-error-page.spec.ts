import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
import { type TransferStatusResponse } from "../../../src/schemas";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";
import { TransferMaterialsDestinationPage } from "../pages/transfer-material-destination";
import { Page } from "@playwright/test";

const MOCK_TRANSFER_ID = "00000000-0000-4000-8000-000000000001";
const BASE_TRANSFER_STATUS = {
  id: MOCK_TRANSFER_ID,
  startedAt: null,
  successfulFiles: 0,
  failedFiles: 0,
};

test.describe("transfer-error-page", () => {
  const startTransfer = async (page: Page) => {
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await page.goto("/case/12/case-management");
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      false,
    );
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.verifyEgressTransferSourceElements();

    await transferMaterialsSourcePage.verifyFolderPath([
      "Egress: Thunderstruck",
    ]);
    await transferMaterialsSourcePage.handleFolderClick("folder-1-0");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      false,
    );
    await transferMaterialsSourcePage.verifyFolderPath([
      "Egress: Thunderstruck",
      "folder-1-0",
    ]);

    await transferMaterialsSourcePage.toggleCheckbox(0);
    await transferMaterialsSourcePage.verifyCopyBtnEnabled(true);
    await transferMaterialsSourcePage.clickCopyBtn();
    const transferMaterialsDestinationPage =
      new TransferMaterialsDestinationPage(page);
    await transferMaterialsDestinationPage.verifyUrl(
      "/case/12/case-management/transfer-destination-page",
    );
    await transferMaterialsDestinationPage.verifyPageElements(
      "egress",
      3,
      "copy",
    );
    await transferMaterialsDestinationPage.verifyFolderExpanded(
      "Shared Drive: Thunderstruck",
      true,
      ["folder-1-0", "folder-1-1"],
    );
    await transferMaterialsDestinationPage.selectFolder("folder-1-0");
    await transferMaterialsDestinationPage.verifyTransferActionEnabled(true);
    await transferMaterialsDestinationPage.verifyTransferActionButtonName(
      `Copy to folder-1-0`,
    );
    await transferMaterialsDestinationPage.clickTransferActionButton();
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
            ...BASE_TRANSFER_STATUS,
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
            totalFiles: 4,
            processedFiles: 4,
            failedFiles: 3,
            successfulItems: [],
            destinationPath: "",
          } as TransferStatusResponse);
        },
      ),
    );
    await startTransfer(page);

    // await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials").locator("h2", {
    //     hasText: "Transfer materials to the Shared Drive",
    //   }),
    // ).not.toBeVisible();
    // await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    // await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials"),
    // ).toContainText("Indexing transfer from Egress to Shared Drive...");
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials"),
    // ).toContainText("Completing transfer from Egress to Shared Drive...");
    // await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
    await expect(page).toHaveURL("/case/12/case-management", {
      timeout: 50000,
    });
    await expect(page).toHaveURL("/case/12/case-management/transfer-errors", {
      timeout: 50000,
    });
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
            ...BASE_TRANSFER_STATUS,
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
            totalFiles: 4,
            processedFiles: 4,
            failedFiles: 3,
            successfulItems: [],
            destinationPath: "",
          } as TransferStatusResponse);
        },
      ),
    );
    // await startTransfer(page);
    // await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials").locator("h2", {
    //     hasText: "Transfer materials to the Shared Drive",
    //   }),
    // ).not.toBeVisible();
    // await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    // await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials"),
    // ).toContainText("Indexing transfer from Egress to Shared Drive...");
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials"),
    // ).toContainText("Completing transfer from Egress to Shared Drive...");
    // await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
    // await expect(page).toHaveURL("/case/12/case-management/transfer-errors");
    await startTransfer(page);
    await expect(page).toHaveURL("/case/12/case-management", {
      timeout: 50000,
    });
    await expect(page).toHaveURL("/case/12/case-management/transfer-errors", {
      timeout: 50000,
    });
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
            ...BASE_TRANSFER_STATUS,
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
            totalFiles: 4,
            processedFiles: 4,
            failedFiles: 3,
            successfulItems: [],
            destinationPath: "",
          } as TransferStatusResponse);
        },
      ),
    );
    // await startTransfer(page);
    // await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials").locator("h2", {
    //     hasText: "Transfer materials to the Shared Drive",
    //   }),
    // ).not.toBeVisible();
    // await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    // await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials"),
    // ).toContainText("Indexing transfer from Egress to Shared Drive...");
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials"),
    // ).toContainText("Completing transfer from Egress to Shared Drive...");
    // await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
    // await expect(page).toHaveURL("/case/12/case-management/transfer-errors");
    await startTransfer(page);
    await expect(page).toHaveURL("/case/12/case-management", {
      timeout: 50000,
    });
    await expect(page).toHaveURL("/case/12/case-management/transfer-errors", {
      timeout: 50000,
    });
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
