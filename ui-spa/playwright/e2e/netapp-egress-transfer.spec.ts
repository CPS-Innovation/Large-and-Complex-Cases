import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
test.describe("netapp-egress-transfer", () => {
  test.describe("netapp to egress : transfer selection, confirmation and happy path", () => {
    test.beforeEach(async ({ page }) => {
      await page.goto("/case/12/case-management");
      await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
      await expect(page.getByTestId("tab-active")).toHaveText(
        "Transfer materials",
      );
      await page
        .getByRole("button", { name: "from the Shared Drive to Egress" })
        .click();

      await expect(
        page.getByTestId("tab-content-transfer-materials").locator("h2"),
      ).toHaveText("Transfer materials to Egress");
    });
    test("Should show and hide the actions button and current folder text indent in the egress side based on the files and folders selected on the netapp side ", async ({
      page,
    }) => {
      await page
        .getByTestId("egress-table-wrapper")
        .locator('role=button[name="folder-1-0"]')
        .click();
      const checkboxes = page
        .getByTestId("netapp-table-wrapper")
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
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toHaveText(
        "Transfer to folder-1-0Copy",
      );
      await checkboxes.nth(0).uncheck();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).not.toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).not.toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).not.toBeVisible();

      await checkboxes.nth(1).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toHaveText(
        "Transfer to folder-1-0Copy",
      );
      await checkboxes.nth(1).uncheck();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).not.toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).not.toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).not.toBeVisible();

      await checkboxes.nth(3).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toHaveText(
        "Transfer to folder-1-0Copy",
      );
      await checkboxes.nth(3).uncheck();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).not.toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).not.toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).not.toBeVisible();
    });
    test("Should show the netapp to egress transfer confirmation pop up with correct texts when user chooses copy operation from inset text", async ({
      page,
    }) => {
      await page
        .getByTestId("egress-table-wrapper")
        .locator('role=button[name="folder-1-0"]')
        .click();
      const checkboxes = page
        .getByTestId("netapp-table-wrapper")
        .locator('input[type="checkbox"]');
      await checkboxes.nth(0).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toBeVisible();
      await page
        .getByTestId("egress-inset-text")
        .getByRole("button", { name: "Copy" })
        .click();
      const confirmationModal = await page.getByTestId("div-modal");
      await expect(confirmationModal).toBeVisible();
      await expect(confirmationModal).toContainText("Confirm");
      await expect(
        confirmationModal.getByLabel(
          "I want to copy 2 folders and 2 files to folder-1-0",
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
        .getByTestId("egress-inset-text")
        .getByRole("button", { name: "Copy" })
        .click();
      await expect(confirmationModal).toBeVisible();
      await expect(
        confirmationModal.getByLabel("I want to copy 1 folder to folder-1-0"),
      ).toBeVisible();
      await confirmationModal.getByRole("button", { name: "Cancel" }).click();
      await checkboxes.nth(2).check();
      await page
        .getByTestId("egress-inset-text")
        .getByRole("button", { name: "Copy" })
        .click();
      await expect(confirmationModal).toBeVisible();
      await expect(
        confirmationModal.getByLabel("I want to copy 2 folders to folder-1-0"),
      ).toBeVisible();
      await confirmationModal.getByRole("button", { name: "Cancel" }).click();
      await expect(confirmationModal).not.toBeVisible();
    });
    test("Should show the netapp to egress transfer confirmation pop up with correct texts when user chooses copy operation from the action dropdown", async ({
      page,
    }) => {
      await page
        .getByTestId("egress-table-wrapper")
        .locator('role=button[name="folder-1-0"]')
        .click();
      const checkboxes = page
        .getByTestId("netapp-table-wrapper")
        .locator('input[type="checkbox"]');
      await checkboxes.nth(0).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toBeVisible();
      await page.getByTestId("transfer-actions-dropdown-0").click();
      await expect(page.getByTestId("dropdown-panel")).toBeVisible();
      await page.getByTestId("transfer-actions-dropdown-0").click();
      await expect(page.getByTestId("dropdown-panel")).not.toBeVisible();
      await page.getByTestId("transfer-actions-dropdown-0").click();
      await expect(
        page
          .getByTestId("dropdown-panel")
          .getByRole("button", { name: "Move" }),
      ).not.toBeVisible();
      await page
        .getByTestId("dropdown-panel")
        .getByRole("button", { name: "Copy" })
        .click();

      const confirmationModal = await page.getByTestId("div-modal");
      await expect(confirmationModal).toBeVisible();
      await expect(confirmationModal).toContainText("Confirm");
      await expect(
        confirmationModal.getByLabel(
          "I want to copy 2 folders and 2 files to folder-2-0",
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
      await expect(
        page
          .getByTestId("dropdown-panel")
          .getByRole("button", { name: "Move" }),
      ).not.toBeVisible();
      await page
        .getByTestId("dropdown-panel")
        .getByRole("button", { name: "Copy" })
        .click();
      await expect(confirmationModal).toBeVisible();
      await expect(confirmationModal).toContainText("Confirm");
      await expect(
        confirmationModal.getByLabel(
          "I want to copy 2 folders and 2 files to folder-2-1",
        ),
      ).toBeVisible();
      await confirmationModal.getByRole("button", { name: "Cancel" }).click();
      await page
        .getByTestId("egress-table-wrapper")
        .getByRole("button", { name: "folder-2-1" })
        .click();
      await page.getByTestId("transfer-actions-dropdown-0").click();
      await expect(
        page
          .getByTestId("dropdown-panel")
          .getByRole("button", { name: "Move" }),
      ).not.toBeVisible();
      await page
        .getByTestId("dropdown-panel")
        .getByRole("button", { name: "Copy" })
        .click();
      const newConfirmationModal = await page.getByTestId("div-modal");
      await expect(newConfirmationModal).toBeVisible();
      await expect(newConfirmationModal).toContainText("Confirm");
      await expect(
        newConfirmationModal.getByLabel(
          "I want to copy 2 folders and 2 files to folder-3-0",
        ),
      ).toBeVisible();
    });

    test("Shows the duplicate warning in the netapp to egress transfer confirmation modal when there are duplicate file/folders with same name in the destination folder", async ({
      page,
    }) => {
      await page
        .getByTestId("netapp-table-wrapper")
        .locator('role=button[name="folder-1-0"]')
        .click();
      await page
        .getByTestId("egress-table-wrapper")
        .locator('role=button[name="folder-1-0"]')
        .click();
      const checkboxes = page
        .getByTestId("netapp-table-wrapper")
        .locator('input[type="checkbox"]');
      await checkboxes.nth(0).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toBeVisible();
      await page
        .getByTestId("egress-inset-text")
        .getByRole("button", { name: "Copy" })
        .click();

      const confirmationModal = await page.getByTestId("div-modal");
      await expect(confirmationModal).toBeVisible();
      await expect(confirmationModal).toContainText("Items already exist");
      await expect(
        confirmationModal.getByLabel(
          "I want to copy 2 folders and 2 files to folder-1-0",
        ),
      ).toBeVisible();
      await expect(
        confirmationModal.getByTestId("duplicate-warning"),
      ).toBeVisible();
      await expect(
        confirmationModal.getByTestId("duplicate-folder-file-list"),
      ).not.toBeVisible();
      await expect(confirmationModal.locator("details>summary")).toHaveText(
        "View items",
      );
      await confirmationModal.locator("details>summary").click();
      await expect(
        confirmationModal.getByTestId("duplicate-folder-file-list"),
      ).toBeVisible();
      await confirmationModal.getByRole("button", { name: "Cancel" }).click();
      await page
        .getByTestId("egress-table-wrapper")
        .locator('role=button[name="folder-2-1"]')
        .click();
      await page
        .getByTestId("egress-inset-text")
        .getByRole("button", { name: "Copy" })
        .click();
      await expect(confirmationModal).toBeVisible();
      await expect(confirmationModal).toContainText("Confirm");
      await expect(confirmationModal).not.toContainText("Items already exist");
      await expect(
        confirmationModal.getByLabel(
          "I want to copy 2 folders and 2 files to folder-2-1",
        ),
      ).toBeVisible();
      await expect(
        confirmationModal.getByTestId("duplicate-warning"),
      ).not.toBeVisible();
      await expect(
        confirmationModal.locator("details>summary"),
      ).not.toBeVisible();
    });
    test("Should successfully complete netapp to egress transfer, copy operation", async ({
      page,
    }) => {
      await page
        .getByTestId("egress-table-wrapper")
        .locator('role=button[name="folder-1-0"]')
        .click();
      const checkboxes = page
        .getByTestId("netapp-table-wrapper")
        .locator('input[type="checkbox"]');
      await checkboxes.nth(0).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).toBeVisible();
      await page
        .getByTestId("egress-inset-text")
        .getByRole("button", { name: "Copy" })
        .click();
      const confirmationModal = await page.getByTestId("div-modal");
      await expect(confirmationModal).toBeVisible();
      await expect(confirmationModal).toContainText("Confirm");
      await expect(
        confirmationModal.getByLabel(
          "I want to copy 2 folders and 2 files to folder-1-0",
        ),
      ).toBeVisible();
      await expect(
        confirmationModal.getByRole("button", { name: "Continue" }),
      ).toBeDisabled();
      await confirmationModal
        .getByLabel("I want to copy 2 folders and 2 files to folder-1-0")
        .click();
      await expect(
        confirmationModal.getByRole("button", { name: "Continue" }),
      ).not.toBeDisabled();
      await confirmationModal.getByRole("button", { name: "Continue" }).click();
      await expect(confirmationModal).not.toBeVisible();
      await expect(page.getByTestId("transfer-spinner")).toBeVisible();
      await expect(
        page.getByTestId("tab-content-transfer-materials").locator("h2", {
          hasText: "Transfer materials to Egress",
        }),
      ).not.toBeVisible();
      await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
      await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
      await expect(
        page.getByTestId("tab-content-transfer-materials"),
      ).toContainText("Indexing transfer from Shared Drive to Egress...");
      await expect(
        page.getByTestId("tab-content-transfer-materials"),
      ).toContainText("Completing transfer from Shared Drive to Egress...");
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
      ).toHaveText("Transfer materials to Egress");
      await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
      await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
    });
    test("Should not allow copying to root egress folder", async ({ page }) => {
      const checkboxes = page
        .getByTestId("netapp-table-wrapper")
        .locator('input[type="checkbox"]');
      await checkboxes.nth(0).check();
      await expect(
        page.getByTestId("transfer-actions-dropdown-0"),
      ).toBeVisible();
      await expect(
        page.getByTestId("transfer-actions-dropdown-1"),
      ).toBeVisible();
      await expect(page.getByTestId("egress-inset-text")).not.toBeVisible();
    });
  });

  test("Should show the netapp to egress transfer loading screen, if the same user comes back to the application after triggering transfer and should show completion as it happens", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: "12",
          egressWorkspaceId: "egress_1",
          netappFolderPath: "netapp/",
          operationName: "Thunderstruck",
          urn: "45AA2098221",
          activeTransferId: "mock-transfer-id",
        });
      }),
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "Initiated",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
            totalFiles: 0,
            processedFiles: 0,
          });
        },
      ),
    );
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
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
    ).toContainText("Completing transfer from Shared Drive to Egress...");
    await expect(
      page.getByTestId("transfer-progress-metrics"),
    ).not.toBeVisible();
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "Initiated",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
            totalFiles: 10,
            processedFiles: 0,
          });
        },
      ),
    );

    await expect(page.getByTestId("transfer-progress-metrics")).toBeVisible();
    await expect(page.getByTestId("transfer-progress-metrics")).toContainText(
      "total files : 10files processed : 0",
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "InProgress",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
            totalFiles: 30,
            processedFiles: 20,
          });
        },
      ),
    );
    await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer materials to Egress",
      }),
    ).not.toBeVisible();
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Completing transfer from Shared Drive to Egress...");
    await expect(page.getByTestId("transfer-progress-metrics")).toBeVisible();
    await expect(page.getByTestId("transfer-progress-metrics")).toContainText(
      "total files : 30files processed : 20",
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "Completed",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
          });
        },
      ),
    );
    await page.waitForTimeout(500);
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
    ).toHaveText("Transfer materials to the Shared Drive");
    await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
  });
  test("Should show the netapp to egress transfer loading screen, if another user come to the application when an active transfer is happening and show the transfer tables once the transfer has completed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: "12",
          egressWorkspaceId: "egress_1",
          netappFolderPath: "netapp/",
          operationName: "Thunderstruck",
          urn: "45AA2098221",
          activeTransferId: "mock-transfer-id",
        });
      }),
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "Initiated",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "abc@example.org",
          });
        },
      ),
    );
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer materials to Egress",
      }),
    ).not.toBeVisible();
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("abc@example.org is currently transferring");
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            status: "InProgress",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "abc@example.org",
          });
        },
      ),
    );
    await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer materials to Egress",
      }),
    ).not.toBeVisible();
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("abc@example.org is currently transferring");
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          return HttpResponse.json({
            status: "Completed",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "abc@example.org",
          });
        },
      ),
    );
    await page.waitForTimeout(500);
    await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
    await expect(
      page.getByTestId("transfer-success-notification-banner"),
    ).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2").nth(0),
    ).toHaveText("Transfer materials to the Shared Drive");
    await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
  });
});
