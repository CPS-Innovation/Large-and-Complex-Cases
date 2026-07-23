import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
import { Page } from "@playwright/test";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";
import { TransferMaterialsDestinationPage } from "../pages/transfer-material-destination";

const MOCK_TRANSFER_ID = "00000000-0000-4000-8000-000000000001";
const BASE_TRANSFER_STATUS = {
  id: MOCK_TRANSFER_ID,
  startedAt: null,
  successfulFiles: 0,
  failedFiles: 0,
};

test.describe("egress-netapp-transfer-indexing-error", () => {
  const startTransfer = async (
    page: Page,
    skipPageElementValidation: boolean = false,
    transferType: "copy" | "move" = "copy",
  ) => {
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);

    if (!skipPageElementValidation) {
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
    }
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
    if (transferType === "copy") {
      await transferMaterialsSourcePage.clickCopyBtn();
    } else {
      await transferMaterialsSourcePage.clickMoveBtn();
    }
    const transferMaterialsDestinationPage =
      new TransferMaterialsDestinationPage(page);
    await transferMaterialsDestinationPage.verifyUrl(
      "/case/12/case-management/transfer-destination-page",
    );
    await transferMaterialsDestinationPage.verifyPageElements(
      "egress",
      3,
      transferType,
    );
    await transferMaterialsDestinationPage.verifyFolderExpanded(
      "Shared Drive: Thunderstruck",
      true,
      ["folder-1-0", "folder-1-1"],
    );
    await transferMaterialsDestinationPage.selectFolder("folder-1-0");
    await transferMaterialsDestinationPage.verifyTransferActionEnabled(true);
    await transferMaterialsDestinationPage.verifyTransferActionButtonName(
      `${transferType === "copy" ? "Copy" : "Move"} to folder-1-0`,
    );
    await transferMaterialsDestinationPage.clickTransferActionButton();
  };

  test.describe("Long file path issue", () => {
    const validateFilePathErrorSection = async (
      page: Page,
      index: number,
      expectedData: {
        relativePath: string;
        filePaths: {
          fileName: string;
          characterText: string;
        }[];
      },
    ) => {
      const sections = await page.locator("section").all();
      await expect(sections[index].getByTestId("relative-path")).toHaveText(
        expectedData.relativePath,
      );
      await expect(sections[index].locator("ul").locator("li")).toHaveCount(
        expectedData.filePaths.length,
      );
      const section2ListItems = await sections[index]
        .locator("ul")
        .locator("li")
        .all();
      expect(section2ListItems).toHaveLength(expectedData.filePaths.length);
      expectedData.filePaths.forEach(async (_data, index) => {
        await expect(
          section2ListItems[index].getByTestId("file-name-wrapper"),
        ).toHaveText(expectedData.filePaths[index].fileName);
        await expect(
          section2ListItems[index].getByTestId("character-tag"),
        ).toHaveText(expectedData.filePaths[index].characterText);
        await expect(
          section2ListItems[index].getByRole("button", { name: "rename" }),
        ).not.toBeDisabled();
      });
    };

    test("Should successfully handle egress to netapp transfer long file path issue", async ({
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
      await worker.use(
        http.get(
          "https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status",
          async () => {
            await delay(10);
            return HttpResponse.json({
              ...BASE_TRANSFER_STATUS,
              status: "InProgress",
              transferType: "Copy",
              direction: "EgressToNetApp",
              completedAt: null,
              failedItems: [],
              userName: "dev_user@example.org",
              totalFiles: 2,
              processedFiles: 1,
              successfulItems: [],
              destinationPath: "",
            });
          },
        ),
      );
      await page.goto("/case/12/case-management?transfer-materials-v1=true");
      await startTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText(
        "File paths are too long to transfer",
      );
      await expect(
        page.getByTestId("resolve-file-path-inset-text"),
      ).toBeVisible();
      await expect(
        page
          .getByTestId("resolve-file-path-inset-text")
          .getByText("2 files exceed the 260 character limit."),
      ).toBeVisible();

      await expect(page.locator("section")).toHaveCount(2);
      const sections = await page.locator("section").all();

      await validateFilePathErrorSection(page, 0, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder3/",
        filePaths: [
          {
            fileName:
              "file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "261 characters",
          },
        ],
      });

      await validateFilePathErrorSection(page, 1, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder4/folder5/",
        filePaths: [
          {
            fileName:
              "file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
            characterText: "266 characters",
          },
        ],
      });

      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).toBeDisabled();

      await expect(
        page.getByTestId("resolve-path-success-notification-banner"),
      ).not.toBeVisible();

      await sections[0].getByRole("button", { name: "rename" }).click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-rename-file",
      );
      await expect(page.locator("h1")).toHaveText("Rename file");

      await expect(await page.locator(`input`)).toHaveValue(
        "file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
      );
      await expect(page.getByTestId("character-tag")).toHaveText(
        "261 characters",
      );

      await expect(
        page.getByText("File path length:261 characters"),
      ).toBeVisible();

      await expect(
        page.getByText("You must reduce this to 260 characters or fewer."),
      ).toBeVisible();

      await expect(
        page.getByRole("button", { name: "Continue" }),
      ).not.toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();

      await page
        .locator(`input`)
        .fill("file3eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf");
      await expect(page.getByTestId("character-tag")).toHaveText(
        "257 characters",
      );
      await expect(
        page.getByText("You must reduce this to 260 characters or fewer."),
      ).not.toBeVisible();

      await page.getByRole("button", { name: "Continue" }).click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );

      await validateFilePathErrorSection(page, 0, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder3/",
        filePaths: [
          {
            fileName:
              "file3eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "257 characters",
          },
        ],
      });
      await validateFilePathErrorSection(page, 1, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder4/folder5/",
        filePaths: [
          {
            fileName:
              "file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
            characterText: "266 characters",
          },
        ],
      });

      await expect(
        page.getByTestId("resolve-path-success-notification-banner"),
      ).not.toBeVisible();
      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).toBeDisabled();

      await sections[1].getByRole("button", { name: "rename" }).click();

      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-rename-file",
      );
      await expect(page.locator("h1")).toHaveText("Rename file");

      await expect(page.locator("input")).toHaveValue(
        "file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
      );
      await expect(
        page.getByText("File path length:266 characters"),
      ).toBeVisible();
      await expect(page.getByTestId("character-tag")).toHaveText(
        "266 characters",
      );

      await expect(
        page.getByText("You must reduce this to 260 characters or fewer."),
      ).toBeVisible();

      await expect(
        page.getByRole("button", { name: "Continue" }),
      ).not.toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();
      await page
        .locator("input")
        .fill("file5eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwsswees.pdf");
      await expect(page.getByTestId("character-tag")).toHaveText(
        "260 characters",
      );
      await expect(
        page.getByText("You must reduce this to 260 characters or fewer."),
      ).not.toBeVisible();

      await page.getByRole("button", { name: "Continue" }).click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await validateFilePathErrorSection(page, 0, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder3/",
        filePaths: [
          {
            fileName:
              "file3eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "257 characters",
          },
        ],
      });
      await validateFilePathErrorSection(page, 1, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder4/folder5/",
        filePaths: [
          {
            fileName: "file5eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwsswees.pdf",
            characterText: "260 characters",
          },
        ],
      });

      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).not.toBeDisabled();
      await expect(
        page.getByTestId("resolve-path-success-notification-banner"),
      ).toBeVisible();
      await expect(
        page.getByRole("heading", {
          name: "File paths are too long to transfer",
        }),
      ).not.toBeVisible();
      await expect(
        page.getByTestId("resolve-file-path-inset-text"),
      ).not.toBeVisible();
      await expect(
        page.getByTestId("resolve-path-success-notification-banner"),
      ).toContainText("Success");
      await expect(
        page.getByTestId("resolve-path-success-notification-banner"),
      ).toContainText("Your files can now be transferred.");

      await worker.use(
        http.get(
          "https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status",
          async () => {
            await delay(10);
            return HttpResponse.json({
              id: "00000000-0000-4000-8000-000000000001",
              startedAt: null,
              failedFiles: 0,
              status: "Completed",
              transferType: "Copy",
              direction: "NetAppToEgress",
              completedAt: null,
              failedItems: [],
              userName: "dev_user@example.org",
              totalFiles: 30,
              processedFiles: 30,
              successfulFiles: 30,
              successfulItems: [
                {
                  sourcePath: "folder1/folder2/file1.txt",
                },
                {
                  sourcePath: "folder1/folder2/file2.txt",
                },
                {
                  sourcePath: "folder1/folder3/file3.txt",
                },
              ],
              destinationPath: "folder-1-0/folder-2-0/",
            });
          },
        ),
      );
      await page.getByRole("button", { name: "Start transfer" }).click();
      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).toBeDisabled();
      await expect(page.getByRole("button", { name: "Cancel" })).toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Rename" }).nth(0),
      ).toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Rename" }).nth(1),
      ).toBeDisabled();

      await expect(page).toHaveURL("/case/12/case-management");

      await delay(500);
      const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
      await transferMaterialsSourcePage.verifyTransferLoaderHidden();
      await transferMaterialsSourcePage.verifyTransferStatsHidden();
      await transferMaterialsSourcePage.validateTransferSuccessBanner(
        [
          {
            folderPath: "folder2",
            files: ["file1.txt", "file2.txt"],
          },
          {
            folderPath: "folder3",
            files: ["file3.txt"],
          },
        ],
        "copy",
      );

      await transferMaterialsSourcePage.verifyPageElements();
      await transferMaterialsSourcePage.verifyEgressTransferSourceElements();
    });
    test("The back link and cancel button from the resolve transfer path files page should take the user to casemanagement page", async ({
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
              sourceRootFolderPath: "egress/",
              transferDirection: "EgressToNetApp",
              isInvalid: true,
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
      await page.goto("/case/12/case-management?transfer-materials-v1=true");
      await startTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText(
        "File paths are too long to transfer",
      );
      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();
      await expect(page.getByRole("link", { name: "Back" })).not.toBeDisabled();
      await page.getByRole("button", { name: "Cancel" }).click();
      await expect(page).toHaveURL("/case/12/case-management");
      await startTransfer(page);

      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
        { timeout: 50000 },
      );
      await expect(page.locator("h1")).toHaveText(
        "File paths are too long to transfer",
        { timeout: 50000 },
      );
      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();
      await expect(page.getByRole("link", { name: "Back" })).not.toBeDisabled();
      await page.getByRole("link", { name: "Back" }).click();
      await expect(page).toHaveURL("/case/12/case-management");
    });
    test("The back link and cancel button from the rename transfer path files page should take the user to resolve transfer path files page", async ({
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
              sourceRootFolderPath: "egress/",
              transferDirection: "EgressToNetApp",
              isInvalid: true,
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
                  sourcePath: "file2.pdf",
                  relativePath: null,
                  fullFilePath: "egress/folder1/file2.pdf",
                },
              ],
            });
          },
        ),
      );
      await page.goto("/case/12/case-management?transfer-materials-v1=true");
      await startTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText(
        "File paths are too long to transfer",
      );
      const renameBtns = await page
        .getByRole("button", { name: "Rename" })
        .all();
      await renameBtns[0].click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-rename-file",
      );
      await expect(page.locator("h1")).toHaveText("Rename file");
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Continue" }),
      ).not.toBeDisabled();
      await expect(page.getByRole("link", { name: "Back" })).not.toBeDisabled();
      await page.getByRole("button", { name: "Cancel" }).click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText(
        "File paths are too long to transfer",
      );
      await renameBtns[1].click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-rename-file",
      );
      await expect(page.locator("h1")).toHaveText("Rename file");
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Continue" }),
      ).not.toBeDisabled();
      await expect(page.getByRole("link", { name: "Back" })).not.toBeDisabled();
      await page.getByRole("link", { name: "Back" }).click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText(
        "File paths are too long to transfer",
      );
      await page.getByRole("link", { name: "Back" }).click();
      await expect(page).toHaveURL("/case/12/case-management");
    });
    test("Should show the indexing error files correctly", async ({
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
                  id: "id_6",
                  sourcePath:
                    "file6qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  destinationFullPath:
                    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/file6qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_6_1",
                  sourcePath:
                    "file6_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  destinationFullPath:
                    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/file6_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_3",
                  sourcePath:
                    "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  destinationFullPath:
                    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_3_1",
                  sourcePath:
                    "egress/folder3/file3_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  destinationFullPath:
                    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder3/file3_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
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
                {
                  id: "id_5_1",
                  sourcePath:
                    "egress/folder4/folder5/file5_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                  destinationFullPath:
                    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder4/folder5/file5_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
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
                  sourcePath: "file2.pdf",
                  relativePath: null,
                  fullFilePath: "egress/folder1/file2.pdf",
                },
              ],
            });
          },
        ),
      );
      await page.goto("/case/12/case-management?transfer-materials-v1=true");
      await startTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText(
        "File paths are too long to transfer",
      );

      await validateFilePathErrorSection(page, 0, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/",
        filePaths: [
          {
            fileName:
              "file6qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "246 characters",
          },
          {
            fileName:
              "file6_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "248 characters",
          },
        ],
      });

      await validateFilePathErrorSection(page, 1, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder3/",
        filePaths: [
          {
            fileName:
              "file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "261 characters",
          },
          {
            fileName:
              "file3_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "263 characters",
          },
        ],
      });

      await validateFilePathErrorSection(page, 2, {
        relativePath:
          "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/egress/folder4/folder5/",
        filePaths: [
          {
            fileName:
              "file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
            characterText: "266 characters",
          },
          {
            fileName:
              "file5_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",

            characterText: "268 characters",
          },
        ],
      });

      await expect(page.locator("section")).toHaveCount(3);
      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();
    });
    test("Shouldnt be able to directly land on the resolve transfer path files page", async ({
      page,
    }) => {
      await page.goto("/case/12/case-management/transfer-resolve-file-path");
      await expect(page).toHaveURL("/");
      await expect(page.locator("h1")).toHaveText(`Find a case`);
    });
    test("Shouldnt be able to directly land on the transfer rename files page", async ({
      page,
    }) => {
      await page.goto("/case/12/case-management/transfer-rename-file");
      await expect(page).toHaveURL("/");
      await expect(page.locator("h1")).toHaveText(`Find a case`);
    });
  });

  test.describe("Transfer move operation user permissions issue", () => {
    test("Should show user permissions error page when user tries to do a move operation without enough permissions", async ({
      page,
      worker,
    }) => {
      await worker.use(
        http.post(
          "https://mocked-out-api/api/v1/filetransfer/files",
          async () => {
            await delay(500);
            return HttpResponse.json(null, { status: 403 });
          },
        ),
      );
      await page.goto("/case/12/case-management?transfer-materials-v1=true");
      await startTransfer(page, false, "move");
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-permissions-error",
      );
      await expect(page.locator("h1")).toHaveText(
        "You do not have permission to transfer these files from Egress",
      );

      await expect(
        page.getByText(
          "If you think you should have access, contact the egress administrator for the case.",
        ),
      ).toBeVisible();
      await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
      await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
        "href",
        "/case/12/case-management",
      );
      await expect(page.getByTestId("contact-information")).toHaveText(
        "To get help, contact the product team.",
      );
      await expect(
        page.getByRole("button", { name: "Return to the case" }),
      ).toBeVisible();
      await page.getByRole("button", { name: "Return to the case" }).click();
      await expect(page).toHaveURL("/case/12/case-management");
      await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    });
    test("User should not be able to land directly on the transfer permissions error page, it should be redirected to search case page", async ({
      page,
    }) => {
      await page.goto("/case/12/case-management/transfer-permissions-error");
      await expect(page).toHaveURL("/");
    });
  });
});
