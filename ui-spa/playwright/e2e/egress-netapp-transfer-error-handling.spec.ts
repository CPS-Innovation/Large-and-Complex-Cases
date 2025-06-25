import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
test.describe("egress-netapp-transfer-error-handling", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
  });
  test("Should show the error page if the indexing file transfer end point throws an Api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post("https://mocked-out-api/api/filetransfer/files", async () => {
        await delay(10);
        return new HttpResponse(null, { status: 500 });
      }),
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
    await expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    await expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
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
        "Error: An error occurred contacting the server at https://mocked-out-api/api/filetransfer/files: indexing file transfer api failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if the indexing file transfer end point response missing required properties", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post("https://mocked-out-api/api/filetransfer/files", async () => {
        await delay(10);
        return HttpResponse.json({ isInvalid: false });
      }),
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
    await expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    await expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
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
        "Error: Invalid indexing file transfer api response. More details, TypeError: Cannot read properties of undefined (reading 'map')",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if initiate end point throws an Api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/filetransfer/initiate",
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
    await expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    await expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
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
        "Error: An error occurred contacting the server at https://mocked-out-api/api/filetransfer/initiate: initiate file transfer failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if initiate end point returns invalid response", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/filetransfer/initiate",
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
    await expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    await expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
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
        "Error: Invalid initiate transfer response, id does not exist",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if the transfer status endpoint throws an Api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/filetransfer/transfer-id-egress-to-netapp/status",
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
    await expect(page.getByTestId("transfer-actions-dropdown-0")).toBeVisible();
    await expect(page.getByTestId("transfer-actions-dropdown-1")).toBeVisible();
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
    await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Indexing transfer from egress to shared drive...");
    await expect(
      page.getByTestId("tab-content-transfer-materials"),
    ).toContainText("Completing transfer from egress to shared drive...");
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
        "Error: An error occurred contacting the server at https://mocked-out-api/api/filetransfer/transfer-id-egress-to-netapp/status: Getting case transfer status failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
});
