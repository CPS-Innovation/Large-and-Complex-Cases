import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";
import { Page } from "@playwright/test";
import { ActivityLogResponse } from "../../src/common/types/ActivityLogResponse";
test.describe("activity log", () => {
  const goToActivityLog = async (page: Page) => {
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);

    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
    await page.getByTestId("tab-1").click();
    await expect(page.getByTestId("tab-active")).toHaveText("Activity log");
  };

  test("Should show the activity log entries correctly, when the user navigates to the activity log tab", async ({
    page,
  }) => {
    await goToActivityLog(page);

    await expect(page.locator("h2")).toHaveText(`Activity Log`);
    await expect(page.getByTestId("activity-log-last-update")).toHaveText(
      "Last Updated 18/01/2024, 12:46 pm",
    );
  });

  test("Should show the activity log entries correctly, for the `CONNECTION_TO_EGRESS`  and `CONNECTION_TO_NETAPP` transfer type", async ({
    page,
    worker,
  }) => {
    const activityLog: ActivityLogResponse = {
      data: [
        {
          id: "2",
          actionType: "CONNECTION_TO_NETAPP",
          timestamp: "2024-01-18T12:50:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to the Shared drive",
          details: null,
        },
        {
          id: "1",
          actionType: "CONNECTION_TO_EGRESS",
          timestamp: "2024-01-18T12:46:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to Egress",
          details: null,
        },
      ],
    };
    await worker.use(
      http.get("https://mocked-out-api/api/v1/activity/logs", async () => {
        await delay(10);
        return HttpResponse.json(activityLog);
      }),
    );
    await goToActivityLog(page);
    await expect(page.locator("h2")).toHaveText(`Activity Log`);
    await expect(page.getByTestId("activity-log-last-update")).toHaveText(
      "Last Updated 18/01/2024, 12:50 pm",
    );
    const sections = await page
      .getByTestId("activities-timeline")
      .locator("section");

    await expect(sections).toHaveCount(2);
    await expect(sections.nth(0).getByTestId("transfer-tag")).not.toBeVisible();
    await expect(
      sections.nth(0).getByTestId("transfer-status-tag"),
    ).not.toBeVisible();
    await expect(sections.nth(0).locator("h4")).toHaveText(
      "Case connected to the Shared drive",
    );
    await expect(sections.nth(0).getByTestId("activity-user")).toHaveText(
      "by dwight_schrute@cps.gov.uk",
    );
    await expect(sections.nth(0).getByTestId("activity-date")).toHaveText(
      "18/01/2024, 12:50 pm",
    );

    await expect(sections.nth(1).getByTestId("transfer-tag")).not.toBeVisible();
    await expect(
      sections.nth(1).getByTestId("transfer-status-tag"),
    ).not.toBeVisible();
    await expect(sections.nth(1).locator("h4")).toHaveText(
      "Case connected to Egress",
    );
    await expect(sections.nth(1).getByTestId("activity-user")).toHaveText(
      "by dwight_schrute@cps.gov.uk",
    );
    await expect(sections.nth(1).getByTestId("activity-date")).toHaveText(
      "18/01/2024, 12:46 pm",
    );
  });
  test("Should show the activity log entries correctly, for the `TRANSFER_INITIATED` transfer type", async ({
    page,
    worker,
  }) => {
    const activityLog: ActivityLogResponse = {
      data: [
        {
          id: "3",
          actionType: "TRANSFER_INITIATED",
          timestamp: "2024-01-18T13:46:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Initiated transfer from egress to shared drive",
          details: {
            transferId: "transfer-1",
            totalFiles: 2,
            errorFileCount: 0,
            transferedFileCount: 0,
            sourcePath: "egress",
            destinationPath: "netapp/folder2",
            files: [],
            errors: [],
            deletionErrors: [],
          },
        },
        {
          id: "2",
          actionType: "CONNECTION_TO_NETAPP",
          timestamp: "2024-01-18T12:50:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to the Shared drive",
          details: null,
        },
        {
          id: "1",
          actionType: "CONNECTION_TO_EGRESS",
          timestamp: "2024-01-18T12:46:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to Egress",
          details: null,
        },
      ],
    };
    await worker.use(
      http.get("https://mocked-out-api/api/v1/activity/logs", async () => {
        await delay(10);
        return HttpResponse.json(activityLog);
      }),
    );
    await goToActivityLog(page);
    await expect(page.locator("h2")).toHaveText(`Activity Log`);
    await expect(page.getByTestId("activity-log-last-update")).toHaveText(
      "Last Updated 18/01/2024, 1:46 pm",
    );
    const sections = await page
      .getByTestId("activities-timeline")
      .locator("section");

    await expect(sections).toHaveCount(3);
    await expect(sections.nth(0).getByTestId("transfer-tag")).toBeVisible();
    await expect(
      sections.nth(0).getByTestId("transfer-status-tag"),
    ).not.toBeVisible();
    await expect(sections.nth(0).locator("h4")).toHaveText(
      "Initiated transfer from egress to shared drive",
    );
    await expect(sections.nth(0).getByTestId("activity-user")).toHaveText(
      "by dwight_schrute@cps.gov.uk",
    );
    await expect(sections.nth(0).getByTestId("activity-date")).toHaveText(
      "18/01/2024, 1:46 pm",
    );

    await expect(sections.nth(0).getByTestId("transfer-source")).toHaveText(
      "Source:egress",
    );
    await expect(
      sections.nth(0).getByTestId("transfer-destination"),
    ).toHaveText("Destination:netapp > folder2");
  });
  test("Should show the activity log entries correctly, for the copy transfer `TRANSFER_COMPLETED` transfer type", async ({
    page,
    worker,
  }) => {
    const activityLog: ActivityLogResponse = {
      data: [
        {
          id: "4",
          actionType: "TRANSFER_COMPLETED",
          timestamp: "2024-01-18T13:50:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Documents/folders copied from egress to shared drive",
          details: {
            transferId: "transfer-1",
            totalFiles: 6,
            errorFileCount: 2,
            transferedFileCount: 4,
            sourcePath: "egress/folder1",
            destinationPath: "netapp/folder2",
            files: [
              {
                path: "egress/folder1/folder3/file1.pdf",
              },
              {
                path: "egress/folder1/file2.pdf",
              },
              {
                path: "egress/folder1/folder4/folder22/file3.pdf",
              },
              {
                path: "egress/folder1/file4.pdf",
              },
            ],
            errors: [
              {
                path: "egress/folder1/file5.pdf",
              },
              {
                path: "egress/folder1/folder2/file6.pdf",
              },
            ],
            deletionErrors: [],
          },
        },
        {
          id: "3",
          actionType: "TRANSFER_INITIATED",
          timestamp: "2024-01-18T13:46:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Document/folders copying from egress to shared drive",
          details: {
            transferId: "transfer-1",
            totalFiles: 2,
            errorFileCount: 0,
            transferedFileCount: 0,
            sourcePath: "egress",
            destinationPath: "netapp/folder2",
            files: [],
            errors: [],
            deletionErrors: [],
          },
        },
        {
          id: "2",
          actionType: "CONNECTION_TO_NETAPP",
          timestamp: "2024-01-18T12:50:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to the Shared drive",
          details: null,
        },
        {
          id: "1",
          actionType: "CONNECTION_TO_EGRESS",
          timestamp: "2024-01-18T12:46:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to Egress",
          details: null,
        },
      ],
    };
    await worker.use(
      http.get("https://mocked-out-api/api/v1/activity/logs", async () => {
        await delay(10);
        return HttpResponse.json(activityLog);
      }),
    );
    await goToActivityLog(page);
    await expect(page.locator("h2")).toHaveText(`Activity Log`);
    await expect(page.getByTestId("activity-log-last-update")).toHaveText(
      "Last Updated 18/01/2024, 1:50 pm",
    );
    const sections = await page
      .getByTestId("activities-timeline")
      .locator(":scope>section");
    await expect(sections.nth(0).getByTestId("transfer-tag")).toBeVisible();
    await expect(sections.nth(0).getByTestId("transfer-tag")).toHaveText(
      "Transfer",
    );
    await expect(
      sections.nth(0).getByTestId("transfer-status-tag"),
    ).toBeVisible();
    await expect(sections.nth(0).getByTestId("transfer-status-tag")).toHaveText(
      "Completed with errors",
    );

    await expect(sections).toHaveCount(4);
    await expect(sections.nth(0).locator("h4")).toHaveText(
      "Documents/folders copied from egress to shared drive",
    );
    await expect(sections.nth(0).getByTestId("activity-user")).toHaveText(
      "by dwight_schrute@cps.gov.uk",
    );
    await expect(sections.nth(0).getByTestId("activity-date")).toHaveText(
      "18/01/2024, 1:50 pm",
    );

    await expect(sections.nth(0).getByTestId("transfer-source")).toHaveText(
      "Source:egress > folder1",
    );
    await expect(
      sections.nth(0).getByTestId("transfer-destination"),
    ).toHaveText("Destination:netapp > folder2");
    await expect(sections.nth(0)).toContainText(
      "Below is a list of documents/folders copied:",
    );
    await expect(sections.nth(0).locator("details>summary")).toHaveText(
      "View files",
    );
    await expect(
      sections.nth(0).getByTestId("activity-files"),
    ).not.toBeVisible();
    await sections.nth(0).locator("details>summary").click();
    await expect(sections.nth(0).getByTestId("activity-files")).toBeVisible();
    const activityFileSections = await sections
      .nth(0)
      .getByTestId("activity-files")
      .locator("section");

    await expect(activityFileSections).toHaveCount(4);
    await expect(activityFileSections.nth(0).locator("ul")).toHaveCount(2);
    await expect(
      activityFileSections.nth(0).getByTestId("activity-relative-path"),
    ).toHaveText("");
    await expect(
      activityFileSections.nth(0).locator("ul").nth(0).locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(0).locator("ul").nth(1).locator("li"),
    ).toHaveCount(2);
    await expect(
      activityFileSections.nth(0).locator("ul").nth(0).locator("li"),
    ).toHaveText("Failed file5.pdf");
    await expect(
      activityFileSections.nth(0).locator("ul").nth(1).locator("li").nth(0),
    ).toHaveText("file2.pdf");
    await expect(
      activityFileSections.nth(0).locator("ul").nth(1).locator("li").nth(1),
    ).toHaveText("file4.pdf");

    await expect(activityFileSections.nth(1).locator("ul")).toHaveCount(1);
    await expect(
      activityFileSections.nth(1).getByTestId("activity-relative-path"),
    ).toHaveText("folder2");
    await expect(
      activityFileSections.nth(1).locator("ul").locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(1).locator("ul").locator("li"),
    ).toHaveText("Failed file6.pdf");

    await expect(activityFileSections.nth(2).locator("ul")).toHaveCount(1);
    await expect(
      activityFileSections.nth(2).getByTestId("activity-relative-path"),
    ).toHaveText("folder3");
    await expect(
      activityFileSections.nth(1).locator("ul").locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(2).locator("ul").locator("li"),
    ).toHaveText("file1.pdf");

    await expect(activityFileSections.nth(3).locator("ul")).toHaveCount(1);
    await expect(
      activityFileSections.nth(3).getByTestId("activity-relative-path"),
    ).toHaveText("folder4 > folder22");
    await expect(
      activityFileSections.nth(3).locator("ul").locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(3).locator("ul").locator("li"),
    ).toHaveText("file3.pdf");
    await expect(sections.nth(0).locator("button")).toHaveText(
      "Download the list of files (.csv)",
    );
  });
  test("Should show the activity log entries correctly, for the `TRANSFER_FAILED` transfer type", async ({
    page,
    worker,
  }) => {
    const activityLog: ActivityLogResponse = {
      data: [
        {
          id: "4",
          actionType: "TRANSFER_FAILED",
          timestamp: "2024-01-18T13:50:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Documents/folders copied from egress to shared drive",
          details: {
            transferId: "transfer-1",
            totalFiles: 6,
            errorFileCount: 2,
            transferedFileCount: 4,
            sourcePath: "egress/folder1",
            destinationPath: "netapp/folder2",
            files: [
              {
                path: "egress/folder1/folder3/file1.pdf",
              },
              {
                path: "egress/folder1/file2.pdf",
              },
              {
                path: "egress/folder1/folder4/folder22/file3.pdf",
              },
              {
                path: "egress/folder1/file4.pdf",
              },
            ],
            errors: [
              {
                path: "egress/folder1/file5.pdf",
              },
              {
                path: "egress/folder1/folder2/file6.pdf",
              },
            ],
            deletionErrors: [],
          },
        },
        {
          id: "3",
          actionType: "TRANSFER_INITIATED",
          timestamp: "2024-01-18T13:46:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Document/folders copying from egress to shared drive",
          details: {
            transferId: "transfer-1",
            totalFiles: 2,
            errorFileCount: 0,
            transferedFileCount: 0,
            sourcePath: "egress",
            destinationPath: "netapp/folder2",
            files: [],
            errors: [],
            deletionErrors: [],
          },
        },
        {
          id: "2",
          actionType: "CONNECTION_TO_NETAPP",
          timestamp: "2024-01-18T12:50:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to the Shared drive",
          details: null,
        },
        {
          id: "1",
          actionType: "CONNECTION_TO_EGRESS",
          timestamp: "2024-01-18T12:46:10.865517Z",
          userName: "dwight_schrute@cps.gov.uk",
          caseId: "case_1",
          description: "Case connected to Egress",
          details: null,
        },
      ],
    };
    await worker.use(
      http.get("https://mocked-out-api/api/v1/activity/logs", async () => {
        await delay(10);
        return HttpResponse.json(activityLog);
      }),
    );
    await goToActivityLog(page);
    await expect(page.locator("h2")).toHaveText(`Activity Log`);
    await expect(page.getByTestId("activity-log-last-update")).toHaveText(
      "Last Updated 18/01/2024, 1:50 pm",
    );
    const sections = await page
      .getByTestId("activities-timeline")
      .locator(":scope>section");
    await expect(sections.nth(0).getByTestId("transfer-tag")).toBeVisible();
    await expect(sections.nth(0).getByTestId("transfer-tag")).toHaveText(
      "Transfer",
    );
    await expect(
      sections.nth(0).getByTestId("transfer-status-tag"),
    ).toBeVisible();
    await expect(sections.nth(0).getByTestId("transfer-status-tag")).toHaveText(
      "Completed with errors",
    );

    await expect(sections).toHaveCount(4);
    await expect(sections.nth(0).locator("h4")).toHaveText(
      "Documents/folders copied from egress to shared drive",
    );
    await expect(sections.nth(0).getByTestId("activity-user")).toHaveText(
      "by dwight_schrute@cps.gov.uk",
    );
    await expect(sections.nth(0).getByTestId("activity-date")).toHaveText(
      "18/01/2024, 1:50 pm",
    );

    await expect(sections.nth(0).getByTestId("transfer-source")).toHaveText(
      "Source:egress > folder1",
    );
    await expect(
      sections.nth(0).getByTestId("transfer-destination"),
    ).toHaveText("Destination:netapp > folder2");
    await expect(sections.nth(0)).toContainText(
      "Below is a list of documents/folders copied:",
    );
    await expect(sections.nth(0).locator("details>summary")).toHaveText(
      "View files",
    );
    await expect(
      sections.nth(0).getByTestId("activity-files"),
    ).not.toBeVisible();
    await sections.nth(0).locator("details>summary").click();
    await expect(sections.nth(0).getByTestId("activity-files")).toBeVisible();
    const activityFileSections = await sections
      .nth(0)
      .getByTestId("activity-files")
      .locator("section");

    await expect(activityFileSections).toHaveCount(4);
    await expect(activityFileSections.nth(0).locator("ul")).toHaveCount(2);
    await expect(
      activityFileSections.nth(0).getByTestId("activity-relative-path"),
    ).toHaveText("");
    await expect(
      activityFileSections.nth(0).locator("ul").nth(0).locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(0).locator("ul").nth(1).locator("li"),
    ).toHaveCount(2);
    await expect(
      activityFileSections.nth(0).locator("ul").nth(0).locator("li"),
    ).toHaveText("Failed file5.pdf");
    await expect(
      activityFileSections.nth(0).locator("ul").nth(1).locator("li").nth(0),
    ).toHaveText("file2.pdf");
    await expect(
      activityFileSections.nth(0).locator("ul").nth(1).locator("li").nth(1),
    ).toHaveText("file4.pdf");

    await expect(activityFileSections.nth(1).locator("ul")).toHaveCount(1);
    await expect(
      activityFileSections.nth(1).getByTestId("activity-relative-path"),
    ).toHaveText("folder2");
    await expect(
      activityFileSections.nth(1).locator("ul").locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(1).locator("ul").locator("li"),
    ).toHaveText("Failed file6.pdf");

    await expect(activityFileSections.nth(2).locator("ul")).toHaveCount(1);
    await expect(
      activityFileSections.nth(2).getByTestId("activity-relative-path"),
    ).toHaveText("folder3");
    await expect(
      activityFileSections.nth(1).locator("ul").locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(2).locator("ul").locator("li"),
    ).toHaveText("file1.pdf");

    await expect(activityFileSections.nth(3).locator("ul")).toHaveCount(1);
    await expect(
      activityFileSections.nth(3).getByTestId("activity-relative-path"),
    ).toHaveText("folder4 > folder22");
    await expect(
      activityFileSections.nth(3).locator("ul").locator("li"),
    ).toHaveCount(1);
    await expect(
      activityFileSections.nth(3).locator("ul").locator("li"),
    ).toHaveText("file3.pdf");
    await expect(sections.nth(0).locator("button")).toHaveText(
      "Download the list of files (.csv)",
    );
  });
});
