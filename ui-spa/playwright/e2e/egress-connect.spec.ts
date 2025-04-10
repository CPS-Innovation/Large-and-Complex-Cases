import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";
import { caseAreasPlaywright } from "../../src/mocks/data";
test.describe("egress connect", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForResponse(`https://mocked-out-api/api/areas`);
  });

  test("should successfully able to connect to an egress folder", async ({
    page,
  }) => {
    await expect(page.locator("h1")).toHaveText(`Find a case`);
    await expect(page.locator("#case-search-types-3")).toBeChecked();
    const input = await page.getByTestId("search-urn");
    await expect(input).toBeVisible();
    await input.fill("11AA2222233");
    await page.locator('button:text("search")').click();
    await expect(page).toHaveURL("search-results?urn=11AA2222233");
    await expect(page.locator("h1")).toHaveText(
      `Search results for URN "11AA2222233"`,
    );
    await expect(
      page.getByText("2 cases found. Select a case to view more details."),
    ).toBeVisible();
    await page.getByRole("link", { name: "Connect" }).click();

    await expect(page).toHaveURL(
      "/case/12/egress-connect?workspace-name=Thunderstruck1_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select an Egress folder to link to the case`,
    );
    await expect(
      page.getByText("Select a folder from the list to link it to this case."),
    ).toBeVisible();
    await expect(
      page.getByText(
        "If the folder you need is not listed, check that you have the correct permissions in Egress or contact the product team for support.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "There are 4 folders matching the case Thunderstruck1_pl on egress.",
      ),
    ).toBeVisible();
    await expect(page.getByTestId("search-folder-name")).toHaveValue(
      "Thunderstruck1_pl",
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/12/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    await expect(
      page.getByText(
        `Are you sure you want to link "thunderstrike" Egress folder to the case?`,
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        `Once linked, this folder will be used for transferring files related to the case. You can update the linked folder later if needed.`,
      ),
    ).toBeVisible();

    await page.getByRole("link", { name: "Back" }).click();

    await expect(page).toHaveURL(
      "/case/12/egress-connect?workspace-name=Thunderstruck1_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select an Egress folder to link to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await page.getByTestId("radio-egress-connect-no").click();
    await page.locator('button:text("Continue")').click();
    await expect(page).toHaveURL(
      "/case/12/egress-connect?workspace-name=Thunderstruck1_pl",
    );
    await expect(page.locator("h1")).toHaveText(
      `Select an Egress folder to link to the case`,
    );
    await page.getByRole("button", { name: "Connect folder" }).first().click();
    await expect(page).toHaveURL("/case/12/egress-connect/confirmation");
    await expect(page.locator("h1")).toHaveText(`Confirm folder link`);
    page.getByTestId("radio-egress-connect-yes").click();
    await page.locator('button:text("Continue")').click();
    // await expect(page).toHaveURL("/case/12/case-overview/transfer-material");
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText(`Find a case`);
  });
});
