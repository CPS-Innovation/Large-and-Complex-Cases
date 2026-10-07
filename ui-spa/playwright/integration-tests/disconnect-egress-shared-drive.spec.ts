import { expect, test } from "./utils/test";
import { delay, HttpResponse, http } from "msw";
test.describe("disconnect Egress / Shared drive", () => {
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

    await page.getByRole("button", { name: "Disconnect" }).click();
    await page.getByRole("button", { name: "Disconnect Shared Drive" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-confirmation?type=shared-drive",
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

    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page.getByTestId("disconnect-error-summary")).toBeVisible();
    await expect(page.getByTestId("disconnect-radio-link")).toHaveText(
      "Select whether you want to disconnect Shared Drive folder",
    );
    await page.getByLabel("No, keep this folder connected").check();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(
      page.getByTestId("disconnect-error-summary"),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await page.getByRole("button", { name: "Disconnect" }).click();
    await page.getByRole("button", { name: "Disconnect Shared Drive" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-confirmation?type=shared-drive",
    );
    await page.getByLabel("Yes, disconnect this folder").check();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page.getByRole("button", { name: "Continue" })).toBeDisabled();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-success?type=shared-drive",
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
    await page.getByRole("button", { name: "Disconnect" }).click();
    await page.getByRole("button", { name: "Disconnect Shared Drive" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-confirmation?type=shared-drive",
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

  test("Should successfully disconnect Egress case", async ({
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

    await page.getByRole("button", { name: "Disconnect" }).click();
    await page.getByRole("button", { name: "Disconnect Egress" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-confirmation?type=egress",
    );
    await expect(page.locator("h1")).toHaveText(`Disconnect Egress`);
    await expect(page.locator("label").nth(0)).toHaveText(
      "Yes, disconnect an Egress case",
    );
    await expect(page.locator("label").nth(1)).toHaveText(
      "No, keep Egress connected",
    );

    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page.getByTestId("disconnect-error-summary")).toBeVisible();
    await expect(page.getByTestId("disconnect-radio-link")).toHaveText(
      "Select whether you want to disconnect Egress",
    );
    await page.getByLabel("No, keep Egress connected").check();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(
      page.getByTestId("disconnect-error-summary"),
    ).not.toBeVisible();
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(`Thunderstruck`);
    await page.getByRole("button", { name: "Disconnect" }).click();
    await page.getByRole("button", { name: "Disconnect Egress" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-confirmation?type=egress",
    );
    await page.getByLabel("Yes, disconnect an Egress case").check();
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page.getByRole("button", { name: "Continue" })).toBeDisabled();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-success?type=egress",
    );
    await expect(page.locator("h1")).toHaveText(`Egress disconnected`);
    await expect(page.locator("p").nth(0)).toHaveText(
      `You've disconnected an Egress case.`,
    );
    await expect(page.locator("p").nth(1)).toHaveText(
      `You can connect to another Egress case to transfer materials.`,
    );
    await expect(
      page.getByRole("link", { name: "Connect Egress" }),
    ).toHaveAttribute("href", "/search-results?urn=45AA2098221");
    await page.getByRole("link", { name: "Connect Egress" }).click();
    await expect(page).toHaveURL("/search-results?urn=45AA2098221");
  });

  test("Should handle disconnect an Egress case error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.delete(
        "https://mocked-out-api/api/v1/egress/connections",
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
    await page.getByRole("button", { name: "Disconnect" }).click();
    await page.getByRole("button", { name: "Disconnect Egress" }).click();
    await expect(page).toHaveURL(
      "/case/12/case-management/disconnect-confirmation?type=egress",
    );

    await page.getByLabel("Yes, disconnect an Egress case").check();
    await page.getByRole("button", { name: "Continue" }).click();

    await expect(page.locator("h1")).toHaveText(`Could not disconnect Egress`);
    await expect(page.locator("p").nth(0)).toHaveText("Try again.");
    await expect(page.locator("p").nth(1)).toHaveText(
      "If the problem continues, contact the product team for support.",
    );
    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page).toHaveURL("/case/12/case-management");
  });
});
