import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
import { Page, Locator } from "@playwright/test";
test.describe("egress-netapp-transfer-indexing-error", () => {
  const caseManagementPageLoad = async (page: Page) => {
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);

    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2"),
    ).toHaveText("Transfer folders and files between egress and shared drive");
  };
  const startEgressToNetAppTransfer = async (
    page: Page,
    transferType: "Copy" | "Move" = "Copy",
  ) => {
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
      .getByRole("button", { name: transferType })
      .click();
    const confirmationModal = await page.getByTestId("div-modal");
    await expect(confirmationModal).toBeVisible();
    const modaltText =
      transferType === "Copy"
        ? "Copy files to: netapp"
        : "Move files to: netapp";
    await expect(confirmationModal).toContainText(modaltText);
    const modalLabelText =
      transferType === "Copy"
        ? "I confirm I want to copy 2 folders and 1 file to netapp"
        : "I confirm I want to move 2 folders and 1 file to netapp";
    await expect(confirmationModal.getByLabel(modalLabelText)).toBeVisible();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).toBeDisabled();
    await confirmationModal.getByLabel(modalLabelText).click();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).not.toBeDisabled();
    await confirmationModal.getByRole("button", { name: "Continue" }).click();
    await expect(confirmationModal).not.toBeVisible();
    await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer folders and files between egress and shared drive",
      }),
    ).not.toBeVisible();
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Indexing transfer from egress to shared drive...");
  };
  test.describe("Long file path issue", () => {
    const validateFilePathErrorSection = async (
      section: Locator,
      expectedData: {
        relativePath: string;
        filePaths: {
          fileName: string;
          characterText: string;
        }[];
      },
    ) => {
      await expect(section.locator("h2")).toHaveText(expectedData.relativePath);
      await expect(section.locator("ul").locator("li")).toHaveCount(
        expectedData.filePaths.length,
      );
      const section2ListItems = await section.locator("ul").locator("li").all();
      await expect(section2ListItems).toHaveLength(
        expectedData.filePaths.length,
      );
      await expectedData.filePaths.forEach(async (data, index) => {
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
              destinationPath:
                "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination",
              validationErrors: [
                {
                  id: "id_3",
                  sourcePath:
                    "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_5",
                  sourcePath:
                    "egress/folder4/folder5/file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                  errorType: "",
                },
              ],
              files: [
                { id: "id_1", sourcePath: "egress/folder1/file1.pdf" },
                { id: "id_2", sourcePath: "egress/folder1/file2.pdf" },
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
              status: "InProgress",
              transferType: "Copy",
              direction: "EgressToNetApp",
              completedAt: null,
              failedItems: [],
              userName: "dev_user@example.org",
            });
          },
        ),
      );
      await caseManagementPageLoad(page);
      await startEgressToNetAppTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText("File paths are too long");
      await expect(
        page.getByTestId("resolve-file-path-inset-text"),
      ).toBeVisible();
      await expect(
        page
          .getByTestId("resolve-file-path-inset-text")
          .getByText(
            "You cannot complete the transfer because 2 file paths are longer than the shared drive limit of 260 characters.",
          ),
      ).toBeVisible();
      await expect(
        page
          .getByTestId("resolve-file-path-inset-text")
          .getByText(
            "You can fix this by choosing a different destination folder with smaller file path or renaming the file name.",
          ),
      ).toBeVisible();
      await expect(page.locator("section")).toHaveCount(2);
      const sections = await page.locator("section").all();

      await validateFilePathErrorSection(sections[0], {
        relativePath: "egress/folder3",
        filePaths: [
          {
            fileName:
              "file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "261 characters",
          },
        ],
      });

      await validateFilePathErrorSection(sections[1], {
        relativePath: "egress/folder4/folder5",
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
      await expect(page.locator("h1")).toHaveText("Edit file name");

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

      await await page
        .locator(`input`)
        .fill("file3eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf");
      await expect(page.getByTestId("character-tag")).toHaveText(
        "257 characters",
      );

      await page.getByRole("button", { name: "Continue" }).click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );

      await validateFilePathErrorSection(sections[0], {
        relativePath: "egress/folder3",
        filePaths: [
          {
            fileName:
              "file3eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "257 characters",
          },
        ],
      });
      await validateFilePathErrorSection(sections[1], {
        relativePath: "egress/folder4/folder5",
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
      await expect(page.locator("h1")).toHaveText("Edit file name");

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

      await page.getByRole("button", { name: "Continue" }).click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await validateFilePathErrorSection(sections[0], {
        relativePath: "egress/folder3",
        filePaths: [
          {
            fileName:
              "file3eweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
            characterText: "257 characters",
          },
        ],
      });
      await validateFilePathErrorSection(sections[1], {
        relativePath: "egress/folder4/folder5",
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
        page.getByTestId("resolve-path-success-notification-banner"),
      ).toContainText("Success");
      await expect(
        page.getByTestId("resolve-path-success-notification-banner"),
      ).toContainText("All file are now under the 260 character limit");
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
      await expect(
        page.getByTestId("tab-content-transfer-materials"),
      ).toContainText("Completing transfer from egress to shared drive...");
      await worker.use(
        http.get(
          "https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status",
          async () => {
            await delay(10);
            return HttpResponse.json({
              status: "Completed",
              transferType: "Copy",
              direction: "EgressToNetApp",
              completedAt: null,
              failedItems: [],
              userName: "dev_user@example.org",
            });
          },
        ),
      );

      await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
      await expect(
        page.getByTestId("transfer-success-notification-banner"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-success-notification-banner"),
      ).toContainText("Success");
      await expect(
        page.getByTestId("transfer-success-notification-banner"),
      ).toContainText("Files copied successfully");
      await expect(
        page.getByTestId("tab-content-transfer-materials").locator("h2").nth(1),
      ).toHaveText(
        "Transfer folders and files between egress and shared drive",
      );
      await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
      await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
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
              isInvalid: true,
              destinationPath:
                "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination",
              validationErrors: [
                {
                  id: "id_3",
                  sourcePath:
                    "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_5",
                  sourcePath:
                    "egress/folder4/folder5/file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                  errorType: "",
                },
              ],
              files: [
                { id: "id_1", sourcePath: "egress/folder1/file1.pdf" },
                { id: "id_2", sourcePath: "egress/folder1/file2.pdf" },
              ],
            });
          },
        ),
      );
      await caseManagementPageLoad(page);
      await startEgressToNetAppTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText("File paths are too long");
      await expect(
        page.getByRole("button", { name: "Start transfer" }),
      ).toBeDisabled();
      await expect(
        page.getByRole("button", { name: "Cancel" }),
      ).not.toBeDisabled();
      await expect(page.getByRole("link", { name: "Back" })).not.toBeDisabled();
      await page.getByRole("button", { name: "Cancel" }).click();
      await expect(page).toHaveURL("/case/12/case-management");
      await startEgressToNetAppTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText("File paths are too long");
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
    test("The back link and cancel button from the rename transfer path files page should take the user to resolve tranfer path files page", async ({
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
              destinationPath:
                "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination",
              validationErrors: [
                {
                  id: "id_3",
                  sourcePath:
                    "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_5",
                  sourcePath:
                    "egress/folder4/folder5/file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                  errorType: "",
                },
              ],
              files: [
                { id: "id_1", sourcePath: "egress/folder1/file1.pdf" },
                { id: "id_2", sourcePath: "egress/folder1/file2.pdf" },
              ],
            });
          },
        ),
      );
      await caseManagementPageLoad(page);
      await startEgressToNetAppTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText("File paths are too long");
      const renameBtns = await page
        .getByRole("button", { name: "Rename" })
        .all();
      await renameBtns[0].click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-rename-file",
      );
      await expect(page.locator("h1")).toHaveText("Edit file name");
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
      await expect(page.locator("h1")).toHaveText("File paths are too long");
      await renameBtns[1].click();
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-rename-file",
      );
      await expect(page.locator("h1")).toHaveText("Edit file name");
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
      await expect(page.locator("h1")).toHaveText("File paths are too long");
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
              destinationPath:
                "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination",
              validationErrors: [
                {
                  id: "id_6",
                  sourcePath:
                    "file6qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_6_1",
                  sourcePath:
                    "file6_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_3",
                  sourcePath:
                    "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_3_1",
                  sourcePath:
                    "egress/folder3/file3_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
                  errorType: "",
                },
                {
                  id: "id_5",
                  sourcePath:
                    "egress/folder4/folder5/file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                  errorType: "",
                },
                {
                  id: "id_5_1",
                  sourcePath:
                    "egress/folder4/folder5/file5_1qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
                  errorType: "",
                },
              ],
              files: [
                { id: "id_1", sourcePath: "egress/folder1/file1.pdf" },
                { id: "id_2", sourcePath: "egress/folder1/file2.pdf" },
              ],
            });
          },
        ),
      );
      await caseManagementPageLoad(page);
      await startEgressToNetAppTransfer(page);
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-resolve-file-path",
      );
      await expect(page.locator("h1")).toHaveText("File paths are too long");

      const sections = await page.locator("section").all();
      await validateFilePathErrorSection(sections[0], {
        relativePath: "folder-1-0",
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

      await validateFilePathErrorSection(sections[1], {
        relativePath: "egress/folder3",
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

      await validateFilePathErrorSection(sections[2], {
        relativePath: "egress/folder4/folder5",
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

      await caseManagementPageLoad(page);
      await startEgressToNetAppTransfer(page, "Move");
      await expect(page).toHaveURL(
        "/case/12/case-management/transfer-permissions-error",
      );
      await expect(page.locator("h1")).toHaveText(
        "You do not have permission to transfer these files from egress",
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
        "To get help, call the Service Desk 0800 692 6996.",
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
