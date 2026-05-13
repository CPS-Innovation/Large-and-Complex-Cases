import { expect, test } from "./utils/test";
import { delay, HttpResponse, http } from "msw";
test.describe("disconnect-shared-drive", () => {
  test("Should successfully disconnect a shared drive", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.delete(
        "https://mocked-out-api/api/v1/netapp/connections",
        async () => {
          await delay(500);
          return new HttpResponse(null, { status: 200 });
        },
      ),
    );
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );

    await page.getByRole("button", { name: "Disconnect Shared Drive" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-shared-drive-confirmation",
    );
    await expect(page.locator("h1")).toHaveText(
      `Disconnect this Shared Drive folder?`,
    );
    await expect(page.locator("label").nth(0)).toHaveText(
      "Yes, disconnect this folder",
    );
    await expect(page.locator("label").nth(1)).toHaveText(
      "No, keep this folder connected",
    );
    await expect(page.getByRole("link", { name: "Cancel" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Cancel" })).toHaveAttribute(
      "href",
      "/case/12/case-management",
    );
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(
      page.getByTestId("disconnect-shared-drive-error-summary"),
    ).toBeVisible();
    await expect(
      page.getByTestId("disconnect-shared-drive-radio-link"),
    ).toHaveText("Select whether you want to disconnect Shared Drive folder");
    await page.getByLabel("No, keep this folder connected").check();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(
      page.getByTestId("disconnect-shared-drive-error-summary"),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await page.getByRole("button", { name: "Disconnect Shared Drive" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-shared-drive-confirmation",
    );
    await page.getByLabel("Yes, disconnect this folder").check();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page.getByRole("button", { name: "Continue" })).toBeDisabled();
    await expect(page.getByRole("link", { name: "Cancel" })).not.toBeVisible();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-shared-drive-success",
    );
    await expect(page.locator("h1")).toHaveText(`Shared Drive disconnected`);
    await expect(page.locator("p").nth(0)).toHaveText(
      `You've disconnected the Shared Drive folder.`,
    );
    await expect(page.locator("p").nth(1)).toHaveText(
      `You can connect a different folder if you need to.`,
    );
    await expect(
      page.getByRole("link", { name: "Connect a folder" }),
    ).toHaveAttribute("href", "/search-results?urn=45AA2098221");
    await page.getByRole("link", { name: "Connect a folder" }).click();
    await expect(page).toHaveURL("/search-results?urn=45AA2098221");
  });

  test("Should handle disconnect a shared drive error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.delete(
        "https://mocked-out-api/api/v1/netapp/connections",
        async () => {
          await delay(100);
          return new HttpResponse(null, { status: 500 });
        },
      ),
    );
    await page.goto("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await expect(page.getByTestId("tab-active")).toHaveText(
      "Transfer materials",
    );

    await page.getByRole("button", { name: "Disconnect Shared Drive" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-shared-drive-confirmation",
    );

    await page.getByLabel("Yes, disconnect this folder").check();
    await page.getByRole("button", { name: "Continue" }).click();

    await expect(page.locator("h1")).toHaveText(
      `Could not disconnect the Shared Drive folder`,
    );
    await expect(page.locator("p").nth(0)).toHaveText("Try again.");
    await expect(page.locator("p").nth(1)).toHaveText(
      "If the problem continues, contact the product team for support.",
    );
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page).toHaveURL("/case/12/case-management");
  });
});
