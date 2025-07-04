import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
test.describe("egress-netapp-transfer-validation-error", () => {
  test.describe("transfer selection, confirmation and happy path", () => {
    test.beforeEach(async ({ page }) => {
      await page.goto("/case/12/case-management");
      await expect(page.locator("h1")).toHaveText(`Thunderstruck`);

      await expect(page.getByTestId("tab-active")).toHaveText(
        "Transfer materials",
      );
      await expect(
        page.getByTestId("tab-content-transfer-materials").locator("h2"),
      ).toHaveText(
        "Transfer folders and files between egress and shared drive",
      );
      await page
        .getByTestId("egress-table-wrapper")
        .locator('role=button[name="folder-1-0"]')
        .click();
    });
    test("Should successfully handle egress to netapp transfer indexing errors", async ({
      page,
      worker,
    }) => {
      await worker.use(
        http.get("https://mocked-out-api/api/filetransfer/files", async () => {
          await delay(10);
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
        }),
      );
      const checkboxes = page
        .getByTestId("egress-table-wrapper")
        .locator('input[type="checkbox"]');
      await checkboxes.nth(0).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("netapp-inset-text")).toBeVisible();
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
      ).toBeDisabled();
      await confirmationModal
        .getByLabel("I confirm I want to copy 2 folders and 1 file to netapp")
        .click();
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
      await expect(
        page.getByTestId("tab-content-transfer-materials"),
      ).toContainText("Completing transfer from egress to shared drive...");
      // await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
      // await expect(
      //   page.getByTestId("transfer-success-notification-banner"),
      // ).toBeVisible();
      // await expect(
      //   page.getByTestId("transfer-success-notification-banner"),
      // ).toContainText("Success");
      // await expect(
      //   page.getByTestId("transfer-success-notification-banner"),
      // ).toContainText("Files copied successfully");
      // await expect(
      //   page.getByTestId("tab-content-transfer-materials").locator("h2").nth(1),
      // ).toHaveText(
      //   "Transfer folders and files between egress and shared drive",
      // );
      // await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
      // await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
    });

    test("The back link and cancel button from the resolve transfer path files page should take the user to casemanagement page", () => {});
    test("The back link and cancel button from the rename transfer path files page should take the user to resolve tranfer path files page", () => {});
    test("If a file in the egress source root folder has indexing error, the folder name of the egress root folder  should be shown as relative path", () => {});
    test("Shouldnt be able to directly land on the resolve transfer path files page", () => {});
    test("Shouldnt be able to directly land on the rename transfer path files page", () => {});
  });
});
