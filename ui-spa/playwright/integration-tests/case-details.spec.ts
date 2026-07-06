import { expect, test } from "./utils/test";
import { Page } from "@playwright/test";
import { delay, HttpResponse, http } from "msw";

test.describe("Case Details", () => {
  const goToCaseManagement = async (page: Page, searchParam: string = "") => {
    await page.goto(`/case/12/case-management?${searchParam}`);
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);

    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
  };

  test("Should show the case details tab correctly", async ({ page }) => {
    await goToCaseManagement(page);
    await page.getByTestId("tab-2").click();
    await expect(page.getByTestId("tab-active")).toHaveText("Case Details");
  });

  test("Should not show the case details tab if the feature is turned off", async ({
    page,
  }) => {
    await goToCaseManagement(page, "case-details=false");
    await expect(page.getByTestId("tab-2")).not.toBeVisible();
  });

  test("Should show the lead defendant name as h1 if the operation name is empty", async ({
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
          operationName: "null",
          leadDefendantName: "John Doe",
          urn: "45AA2098221",
          activeTransferId: "mock-transfer-id",
        });
      }),
    );
    await page.goto(`/case/12/case-management`);
    await expect(page.locator("h1")).toHaveText("John Doe");

    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );
  });
});
