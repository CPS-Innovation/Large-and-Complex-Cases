import { test } from "../utils/test";
import { type Page, expect } from "@playwright/test";
import { delay, HttpResponse, http } from "msw";
import { TransferMaterialsSourcePage } from "../pages/transfer-material-source";
import { TransferMaterialsDestinationPage } from "../pages/transfer-material-destination";
async function runTransferScenario(page: Page, transferType: "copy" | "move") {
  const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
  await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
  await transferMaterialsSourcePage.verifyPageElements();
  await transferMaterialsSourcePage.verifyEgressTransferSourceElements();
  await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
    "egress",
    true,
  );
  await transferMaterialsSourcePage.verifyTransferSourceTableLoader(
    "egress",
    false,
  );
  await transferMaterialsSourcePage.verifyFolderPath(["Egress: Thunderstruck"]);
  await transferMaterialsSourcePage.validateTableColumnHeaders();

  const folderRows = [
    ["", "folder-1-0", "02/01/2000", "--"],
    ["", "folder-1-1", "03/01/2000", "--"],
    ["", "file-1-2.pdf", "03/01/2000", "1.23 KB"],
  ];
  await transferMaterialsSourcePage.validateTableRowValues(folderRows);
  await transferMaterialsSourcePage.verifyCopyBtnEnabled(false);
  await transferMaterialsSourcePage.verifyMoveBtnEnabled(false);

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
  await transferMaterialsSourcePage.validateTableRowValues([
    ["", "folder-2-0", "02/01/2000", "--"],
    ["", "folder-2-1", "03/01/2000", "--"],
    ["", "file-2-2.pdf", "03/01/2000", "1.23 KB"],
  ]);
  await transferMaterialsSourcePage.verifyCheckboxesVisibility(true, 4);
  await transferMaterialsSourcePage.verifyCopyBtnEnabled(false);
  await transferMaterialsSourcePage.verifyMoveBtnEnabled(false);
  await transferMaterialsSourcePage.toggleCheckbox(0);
  await transferMaterialsSourcePage.verifyCopyBtnEnabled(true);
  await transferMaterialsSourcePage.verifyMoveBtnEnabled(true);

  if (transferType === "copy") {
    await transferMaterialsSourcePage.clickCopyBtn();
  } else {
    await transferMaterialsSourcePage.clickMoveBtn();
  }

  const transferMaterialsDestinationPage = new TransferMaterialsDestinationPage(
    page,
  );
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
    ["netapp-folder-1-0", "netapp-folder-1-1"],
  );
  await transferMaterialsDestinationPage.clickMinimizeFolder(
    "Shared Drive: Thunderstruck",
  );
  await transferMaterialsDestinationPage.verifyFolderExpanded(
    "Shared Drive: Thunderstruck",
    false,
    [],
  );
  await transferMaterialsDestinationPage.clickExpandFolder(
    "Shared Drive: Thunderstruck",
  );
  await transferMaterialsDestinationPage.verifyFolderExpanded(
    "Shared Drive: Thunderstruck",
    true,
    ["netapp-folder-1-0", "netapp-folder-1-1"],
  );
  await transferMaterialsDestinationPage.clickExpandFolder("netapp-folder-1-0");
  await transferMaterialsDestinationPage.verifyTransferDestinationTableLoader(
    true,
  );
  await transferMaterialsDestinationPage.verifyTransferDestinationTableLoader(
    false,
  );
  await transferMaterialsDestinationPage.verifyFolderExpanded(
    "netapp-folder-1-0",
    true,
    ["netapp-folder-2-0", "netapp-folder-2-1"],
  );
  await transferMaterialsDestinationPage.clickMinimizeFolder(
    "netapp-folder-1-0",
  );
  await transferMaterialsDestinationPage.verifyFolderExpanded(
    "netapp-folder-1-0",
    false,
  );
  await transferMaterialsDestinationPage.verifyTransferActionEnabled(false);
  await transferMaterialsDestinationPage.selectFolder("netapp-folder-1-0");
  await transferMaterialsDestinationPage.verifyTransferActionEnabled(true);
  await transferMaterialsDestinationPage.verifyTransferActionButtonName(
    `${transferType === "copy" ? "Copy" : "Move"} to netapp-folder-1-0`,
  );
  await transferMaterialsDestinationPage.clickTransferActionButton();
  await transferMaterialsSourcePage.verifyUrl("/case/12/case-management");
  await transferMaterialsSourcePage.verifyPageElements();
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
    transferType,
  );
}
test.describe("transfer material egress netapp transfer", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/case/12/case-management");
  });

  test("Should successfully handle the copy of materials from egress to shared drive", async ({
    page,
  }) => {
    await runTransferScenario(page, "copy");
  });

  test("Should successfully handle the move of materials from egress to shared drive", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/transfer-id-egress-to-netapp/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            id: "00000000-0000-4000-8000-000000000001",
            status: "Completed",
            transferType: "Move",
            direction: "EgressToNetApp",
            startedAt: null,
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
            totalFiles: 30,
            processedFiles: 30,
            successfulFiles: 30,
            failedFiles: 0,
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
    await runTransferScenario(page, "move");
  });

  test("Should show the egress to netapp transfer loading screen, if the same user come back to the application after triggering transfer and should show completion as it happens", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: 12,
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
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            successfulFiles: 0,
            failedFiles: 0,
            status: "Initiated",
            transferType: "Copy",
            direction: "EgressToNetApp",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
            totalFiles: 0,
            processedFiles: 0,
            successfulItems: [],
            destinationPath: "",
          });
        },
      ),
    );

    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.verifyTransferLoaderVisible(true);
    await transferMaterialsSourcePage.verifyTransferStatsHidden();
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            successfulFiles: 0,
            failedFiles: 0,
            status: "Initiated",
            transferType: "Copy",
            direction: "EgressToNetApp",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
            totalFiles: 10,
            processedFiles: 0,
            successfulItems: [],
            destinationPath: "",
          });
        },
      ),
    );

    await transferMaterialsSourcePage.verifyTransferStats(
      "total files : 10files processed : 0",
    );

    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            successfulFiles: 0,
            failedFiles: 0,
            status: "InProgress",
            transferType: "Copy",
            direction: "EgressToNetApp",
            completedAt: null,
            failedItems: [],
            userName: "dev_user@example.org",
            totalFiles: 30,
            processedFiles: 20,
            successfulItems: [],
            destinationPath: "",
          });
        },
      ),
    );
    await transferMaterialsSourcePage.verifyTransferLoaderVisible(true);
    await transferMaterialsSourcePage.verifyTransferStats(
      "total files : 30files processed : 20",
    );

    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            failedFiles: 0,
            status: "Completed",
            transferType: "Copy",
            direction: "EgressToNetApp",
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
    await page.waitForTimeout(500);
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
  });

  test("Should show the netapp to egress transfer loading screen, if another user come to the application when an active transfer is happening and show the transfer tables once the transfer has completed", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: 12,
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
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            successfulFiles: 0,
            failedFiles: 0,
            status: "Initiated",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "abc@example.org",
            totalFiles: 30,
            processedFiles: 0,
            successfulItems: [],
            destinationPath: "",
          });
        },
      ),
    );
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.verifyTransferLoaderVisible(
      false,
      "abc@example.org",
    );
    await transferMaterialsSourcePage.verifyTransferStats(
      "total files : 30files processed : 0",
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          await delay(10);
          return HttpResponse.json({
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            successfulFiles: 0,
            failedFiles: 0,
            status: "InProgress",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "abc@example.org",
            totalFiles: 30,
            processedFiles: 10,
            successfulItems: [],
            destinationPath: "",
          });
        },
      ),
    );
    await transferMaterialsSourcePage.verifyTransferLoaderVisible(
      false,
      "abc@example.org",
    );
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
        async () => {
          return HttpResponse.json({
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            successfulFiles: 0,
            failedFiles: 0,
            status: "Completed",
            transferType: "Copy",
            direction: "NetAppToEgress",
            completedAt: null,
            failedItems: [],
            userName: "abc@example.org",
            totalFiles: 30,
            processedFiles: 30,
            successfulItems: [],
            destinationPath: "",
          });
        },
      ),
    );
    await page.waitForTimeout(500);
    await transferMaterialsSourcePage.verifyTransferLoaderHidden();
    await transferMaterialsSourcePage.verifyTransferStatsHidden();

    await transferMaterialsSourcePage.validateTransferSuccessBannerHidden();
    await transferMaterialsSourcePage.verifyEgressTransferSourceElements();
  });

  test("Should not show the transfer move option if the transferMove feature flag is disabled", async ({
    page,
  }) => {
    await page.goto("/case/12/case-management?transfer-move=false");
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.verifyEgressTransferSourceElements();
    await transferMaterialsSourcePage.verifyMoveBtnEnabled(false);
  });

  test("Should show the egress connection error screen, if user who does not have access to egress come to the application when there is an active transfer Id", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/egress/workspaces/egress_1/files",
        async () => {
          await delay(500);
          return new HttpResponse(null, { status: 401 });
        },
      ),
    );
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(10);
        return HttpResponse.json({
          caseId: 12,
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
            id: "00000000-0000-4000-8000-000000000001",
            startedAt: null,
            successfulFiles: 0,
            failedFiles: 0,
            status: "InProgress",
            transferType: "Copy",
            direction: "EgressToNetApp",
            completedAt: null,
            failedItems: [],
            userName: "abc@example.org",
            totalFiles: 30,
            processedFiles: 2,
            successfulItems: [],
            destinationPath: "",
          });
        },
      ),
    );

    let statusApiCall = false;
    page.on("request", (request) => {
      if (
        request
          .url()
          .includes(
            "https://mocked-out-api/api/v1/filetransfer/mock-transfer-id/status",
          )
      ) {
        statusApiCall = true;
      }
    });
    await page.goto("/case/12/case-management");
    const transferMaterialsSourcePage = new TransferMaterialsSourcePage(page);
    await transferMaterialsSourcePage.verifyPageElements();
    await transferMaterialsSourcePage.verifyTransferLoaderHidden();

    //Note: convert to connectionError page class
    await expect(page).toHaveURL(
      "/case/12/case-management/connection-error?type=egress",
    );
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to Egress",
    );
    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toHaveText([
      "check the case exists and you have access on the Case Management System",
      "contact the product team if you need help",
    ]);

    expect(statusApiCall).toBe(false);
  });
});
