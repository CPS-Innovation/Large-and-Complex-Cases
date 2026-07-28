import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";
import { type TransferStatusResponse } from "../../../src/schemas";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";
import { TransferMaterialsDestinationPage } from "../pages/transfer-material-destination";
const MOCK_TRANSFER_ID = "00000000-0000-4000-8000-000000000001";
const BASE_TRANSFER_STATUS = {
  id: MOCK_TRANSFER_ID,
  startedAt: null,
  successfulFiles: 0,
  failedFiles: 0,
};
test.describe("Transfer v1 skip link test ", () => {
  test("Transfer Destination & Transfer Error Page -  clicking skip-link should take you to the main content ", async ({
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
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await page.goto("/case/12/case-management?transfer-materials-v1=true");
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      false,
    );
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
    await transferMaterialsSourcePage.clickCopyBtn();
    const transferMaterialsDestinationPage =
      new TransferMaterialsDestinationPage(page);
    await transferMaterialsDestinationPage.verifyUrl(
      "/case/12/case-management/transfer-destination-page",
    );
    await delay(500);
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await page.keyboard.press("Tab");
    await expect(page.getByRole("button", { name: "Cancel" })).toBeFocused();
    await transferMaterialsDestinationPage.selectFolder("folder-1-0");
    await transferMaterialsDestinationPage.verifyTransferActionEnabled(true);
    await transferMaterialsDestinationPage.verifyTransferActionButtonName(
      `Copy to folder-1-0`,
    );
    await transferMaterialsDestinationPage.clickTransferActionButton();

    await expect(page).toHaveURL("/case/12/case-management/transfer-errors", {
      timeout: 50000,
    });
    await expect(page.locator("h1")).toHaveText(
      "There is a problem transferring files",
    );
    await delay(500);
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      page.getByTestId("file-exists-error-wrapper").locator("details>summary"),
    ).toBeFocused();
  });

  test("Transfer Resolve File Path and File Rename Page -  clicking skip-link should take you to the main content ", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/filetransfer/files",
        async () => {
          await delay(500);
          return HttpResponse.json({
            caseId: 12,
            isInvalid: true,
            sourceRootFolderPath: "egress/",
            transferDirection: "EgressToNetApp",
            destinationPath:
              "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/",
            validationErrors: [
              {
                id: "id_3",
                sourcePath:
                  "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                destinationFullPath:
                  "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                errorType: "",
              },
              {
                id: "id_5",
                sourcePath:
                  "egress/folder4/folder5/file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                destinationFullPath:
                  "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder4/folder5/file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                errorType: "",
              },
            ],
            files: [
              {
                id: "id_1",
                sourcePath: "file1.pdf",
                relativePath: null,
                fullFilePath: "egress/folder1/file1.pdf",
              },
              {
                id: "id_2",
                sourcePath: "`file2.pdf",
                relativePath: null,
                fullFilePath: "egress/folder1/file2.pdf",
              },
            ],
          });
        },
      ),
    );

    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await page.goto("/case/12/case-management?transfer-materials-v1=true");
    await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      true,
    );
    await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
      "egress",
      false,
    );
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
    await transferMaterialsSourcePage.clickCopyBtn();
    const transferMaterialsDestinationPage =
      new TransferMaterialsDestinationPage(page);
    await transferMaterialsDestinationPage.verifyUrl(
      "/case/12/case-management/transfer-destination-page",
    );
    await transferMaterialsDestinationPage.selectFolder("folder-1-0");
    await transferMaterialsDestinationPage.verifyTransferActionEnabled(true);
    await transferMaterialsDestinationPage.verifyTransferActionButtonName(
      `Copy to folder-1-0`,
    );
    await transferMaterialsDestinationPage.clickTransferActionButton();
    await expect(page).toHaveURL(
      "/case/12/case-management/transfer-resolve-file-path",
    );
    await delay(500);
    await expect(page.locator("h1")).toHaveText(
      "File paths are too long to transfer",
    );

    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(
      page.getByRole("button", { name: /Rename/i }).first(),
    ).toBeFocused();
    await page
      .getByRole("button", { name: /Rename/i })
      .first()
      .click();
    await expect(page).toHaveURL(
      "/case/12/case-management/transfer-rename-file",
    );
    await delay(500);
    await expect(page.locator("h1")).toHaveText("Rename file");
    await page.keyboard.press("Tab");

    await expect(
      page.getByRole("link", { name: /Skip to main content/i }),
    ).toBeFocused();
    await page.keyboard.press("Enter");
    await expect(page).toHaveURL(/#main-content$/);
    await page.keyboard.press("Tab");
    await expect(await page.locator(`input`)).toBeFocused();
  });
});
