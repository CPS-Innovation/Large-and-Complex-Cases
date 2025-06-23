import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
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
  test("Should successfully complete and egress to netapp transfer", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer folders and files between egress and shared drive",
      }),
    ).toBeVisible();
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
    ).toHaveText("Transfer folders and files between egress and shared drive");
    await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
  });
  test("Should show the egress to netapp transfer loading screen if the same user come back to the application after trigering transfer and should show completion as it happens", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: "12",
          egressWorkspaceId: "egress_1",
          netappFolderPath: "netapp/",
          operationName: "Thunderstruck",
          urn: "45AA2098221",
          transferId: "mock-transfer-id",
        });
      }),
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            overallStatus: "INITIATED",
            transferType: "COPY",
            direction: "EgressToNetApp",
            completedAt: null,
            failedFiles: [],
            username: "dev_user@example.org",
          });
        },
      ),
    );
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
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
    ).toContainText("Completing transfer from egress to shared drive...");
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            overallStatus: "IN_PROGRESS",
            transferType: "COPY",
            direction: "EgressToNetApp",
            completedAt: null,
            failedFiles: [],
            username: "dev_user@example.org",
          });
        },
      ),
    );
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
    ).toContainText("Completing transfer from egress to shared drive...");
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            overallStatus: "COMPLETED",
            transferType: "COPY",
            direction: "EgressToNetApp",
            completedAt: null,
            failedFiles: [],
            username: "dev_user@example.org",
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
    ).toHaveText("Transfer folders and files between egress and shared drive");
    await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
  });
  test("Should show the egress to netapp transfer loading screen if the another user come to the application when a active transfer is happening and show the transfer tables once the transfer has completed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: "12",
          egressWorkspaceId: "egress_1",
          netappFolderPath: "netapp/",
          operationName: "Thunderstruck",
          urn: "45AA2098221",
          transferId: "mock-transfer-id",
        });
      }),
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            overallStatus: "INITIATED",
            transferType: "COPY",
            direction: "EgressToNetApp",
            completedAt: null,
            failedFiles: [],
            username: "abc@example.org",
          });
        },
      ),
    );
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
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
    ).toContainText("abc@example.org is currently transferring");
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            overallStatus: "IN_PROGRESS",
            transferType: "COPY",
            direction: "EgressToNetApp",
            completedAt: null,
            failedFiles: [],
            username: "abc@example.org",
          });
        },
      ),
    );
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
    ).toContainText("abc@example.org is currently transferring");
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            overallStatus: "COMPLETED",
            transferType: "COPY",
            direction: "EgressToNetApp",
            completedAt: null,
            failedFiles: [],
            username: "abc@example.org",
          });
        },
      ),
    );
    await expect(page.getByTestId("transfer-spinner")).not.toBeVisible();
    await expect(
      page.getByTestId("transfer-success-notification-banner"),
    ).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2").nth(0),
    ).toHaveText("Transfer folders and files between egress and shared drive");
    await expect(page.getByTestId("egress-table-wrapper")).toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).toBeVisible();
  });
  test("Should show the error page if the validate end point throws an Api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/filetransfer/validate",
        async () => {
          await delay(10);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer folders and files between egress and shared drive",
      }),
    ).toBeVisible();
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
    ).toBeDisabled();
    await confirmationModal
      .getByLabel("I confirm I want to copy 2 folders and 1 file to netapp")
      .click();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).not.toBeDisabled();
    await confirmationModal.getByRole("button", { name: "Continue" }).click();
    await expect(confirmationModal).not.toBeVisible();
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: An error occurred contacting the server at https://mocked-out-api/api/v1/filetransfer/validate: validating file transfer failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if the validate end point returns invalid response", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/filetransfer/validate",
        async () => {
          await delay(10);
          return HttpResponse.json({ isValid: true });
        },
      ),
    );
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer folders and files between egress and shared drive",
      }),
    ).toBeVisible();
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
    ).toBeDisabled();
    await confirmationModal
      .getByLabel("I confirm I want to copy 2 folders and 1 file to netapp")
      .click();
    await expect(
      confirmationModal.getByRole("button", { name: "Continue" }),
    ).not.toBeDisabled();
    await confirmationModal.getByRole("button", { name: "Continue" }).click();
    await expect(confirmationModal).not.toBeVisible();
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: Invalid validateFileTransfer api response. More details, TypeError: Cannot read properties of undefined (reading 'map')",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if initiate end point throws an Api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/filetransfer/initiate",
        async () => {
          await delay(10);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer folders and files between egress and shared drive",
      }),
    ).toBeVisible();
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
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Indexing transfer from egress to shared drive...");
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: An error occurred contacting the server at https://mocked-out-api/api/v1/filetransfer/initiate: initiate file transfer failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if initiate end point returns invalid response", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/filetransfer/initiate",
        async () => {
          await delay(10);
          return HttpResponse.json({});
        },
      ),
    );
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await expect(
      page.getByTestId("tab-content-transfer-materials").locator("h2", {
        hasText: "Transfer folders and files between egress and shared drive",
      }),
    ).toBeVisible();
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
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Indexing transfer from egress to shared drive...");
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: Invalid initiate transfer response, transferId does not exist",
      ),
    ).toBeVisible();
  });
});
