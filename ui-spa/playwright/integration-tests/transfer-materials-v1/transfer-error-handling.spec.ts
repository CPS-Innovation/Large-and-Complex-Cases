import { expect, test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";
import { TransferMaterialsDestinationPage } from "../pages/transfer-material-destination";
test.describe("transfer-error-handling", () => {
  test.beforeEach(async ({ page }) => {
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
  });
  test("Should show the error page if the indexing file transfer end point throws an Api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/filetransfer/files",
        async () => {
          await delay(10);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
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
        "Error: An error occurred contacting the server at https://mocked-out-api/api/v1/filetransfer/files: indexing file transfer api failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });

  test("Should show the error page if the indexing file transfer end point response missing required properties", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.post(
        "https://mocked-out-api/api/v1/filetransfer/files",
        async () => {
          await delay(10);
          return HttpResponse.json({ isInvalid: false });
        },
      ),
    );
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
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
        "Error: An error occurred contacting the server at https://mocked-out-api/api/v1/filetransfer/files: response schema validation failed; status - OK (200)",
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
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
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

    // await expect(page.getByTestId("transfer-spinner")).toBeVisible();
    // await expect(page.getByTestId("egress-table-wrapper")).not.toBeVisible();
    // await expect(page.getByTestId("netapp-table-wrapper")).not.toBeVisible();
    // await expect(
    //   page.getByTestId("tab-content-transfer-materials"),
    // ).toContainText("Indexing transfer from Egress to Shared Drive...");
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
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
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
        "Error: An error occurred contacting the server at https://mocked-out-api/api/v1/filetransfer/initiate: response schema validation failed; status - OK (200)",
      ),
    ).toBeVisible();
  });
  test("Should show the error page if the transfer status endpoint throws an Api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status",
        async () => {
          await delay(10);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
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
    //   await expect(
    //     page.getByTestId("tab-content-transfer-materials"),
    //   ).toContainText("Indexing transfer from Egress to Shared Drive...");
    //   await expect(
    //     page.getByTestId("tab-content-transfer-materials"),
    //   ).toContainText("Completing transfer from Egress to Shared Drive...");
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
      {
        timeout: 50000,
      },
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: An error occurred contacting the server at https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status: Getting case transfer status failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
});
