import { test, expect, type Page } from "@playwright/test";
import { browserLogin } from "../fixtures/setup-helper";
import { getAzureADToken } from "../helpers/auth-api";
import { loadEnvConfig } from "../helpers/env-config";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";

const config = loadEnvConfig();
const caseId = Number(config.defaultCaseId);
const caseUrn = config.defaultCaseUrn;
const apiBaseUrl = config.lccApiBaseUrl || new URL(config.cmsLoginPage).origin;
const DISCONNECT_EGRESS_ROUTE = "**/api/v1/egress/connections?case-id=*";

let cachedToken: { value: string; expiresAt: number } | undefined;

async function lccToken(): Promise<string> {
  if (cachedToken && Date.now() < cachedToken.expiresAt) return cachedToken.value;
  const request = () =>
    getAzureADToken(config.tenantId, config.lccApiClientId, config.e2eAdUser, config.e2eAdPassword);
  let value: string;
  try {
    value = await request();
  } catch (err) {
    if (!String(err).includes("AADSTS80002")) throw err;
    await new Promise((r) => setTimeout(r, 5_000));
    value = await request();
  }
  cachedToken = { value, expiresAt: Date.now() + 45 * 60 * 1000 };
  return value;
}

async function getCaseEgressWorkspaceId(page: Page): Promise<string | null> {
  const res = await page.request.get(`${apiBaseUrl}/api/v1/cases/${caseId}`, {
    headers: { Authorization: `Bearer ${await lccToken()}`, "Correlation-Id": crypto.randomUUID() },
  });
  expect(res.ok(), `GET case ${caseId} returned ${res.status()}`).toBeTruthy();
  return (await res.json()).egressWorkspaceId ?? null;
}

async function reconnectEgress(page: Page): Promise<void> {
  const res = await page.request.post(`${apiBaseUrl}/api/v1/egress/connections`, {
    headers: { Authorization: `Bearer ${await lccToken()}`, "Correlation-Id": crypto.randomUUID() },
    data: {
      caseId,
      egressWorkspaceId: config.defaultWorkspaceId,
      egressWorkspaceName: config.defaultWorkspaceName,
    },
  });
  expect(res.ok(), `reconnecting Egress returned ${res.status()}`).toBeTruthy();
}

async function openTransferMaterials(page: Page): Promise<void> {
  const caseSearch = new CaseSearchPage(page);
  await caseSearch.searchByUrn(caseUrn);
  const results = new SearchResultsPage(page);
  await results.waitForResults();
  await results.clickCaseAction(caseUrn);
  const caseManagement = new CaseManagementPage(page);
  await caseManagement.waitForLoad();
  await caseManagement.switchToTab("transfer-materials");
}

async function chooseDisconnectEgress(page: Page): Promise<void> {
  await page.locator("#disconnect-actions-dropdown").click();
  await page.getByTestId("dropdown-panel").getByRole("button", { name: "Disconnect Egress" }).click();
  await expect(page).toHaveURL(new RegExp(`/case/${caseId}/case-management/disconnect-confirmation\\?type=egress`));
}

async function answerConfirmation(page: Page, answer: "yes" | "no"): Promise<void> {
  await page.getByTestId(`disconnect-radio-${answer}`).check();
  await page.getByRole("button", { name: "Continue" }).click();
}

test.describe("Disconnect Egress (RLCC-100)", () => {
  test.beforeEach(async ({ page }) => {
    test.setTimeout(240_000);
    await browserLogin(page);
    await openTransferMaterials(page);
  });

  test.afterEach(async ({ page }) => {
    if ((await getCaseEgressWorkspaceId(page)) !== config.defaultWorkspaceId) {
      await reconnectEgress(page);
    }
    expect(await getCaseEgressWorkspaceId(page)).toBe(config.defaultWorkspaceId);
  });

  test("Disconnect menu offers Shared Drive and Egress", async ({ page }) => {
    await page.locator("#disconnect-actions-dropdown").click();
    const panel = page.getByTestId("dropdown-panel");
    await expect(panel.getByRole("button", { name: "Disconnect Shared Drive" })).toBeVisible();
    await expect(panel.getByRole("button", { name: "Disconnect Egress" })).toBeVisible();
  });

  test("confirmation page asks before disconnecting and requires an answer", async ({ page }) => {
    await chooseDisconnectEgress(page);

    await expect(page.locator("h1")).toHaveText("Disconnect Egress?");
    await expect(page.getByText("Yes, disconnect an Egress case")).toBeVisible();
    await expect(page.getByText("No, keep Egress connected")).toBeVisible();

    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page.getByTestId("disconnect-error-summary")).toContainText(
      "Select whether you want to disconnect Egress",
    );
    expect(await getCaseEgressWorkspaceId(page)).toBe(config.defaultWorkspaceId);
  });

  test("No returns to transfer materials and keeps Egress connected", async ({ page }) => {
    await chooseDisconnectEgress(page);
    await answerConfirmation(page, "no");

    await expect(page).toHaveURL(new RegExp(`/case/${caseId}/case-management$`));
    await expect(page.locator("#disconnect-actions-dropdown")).toBeVisible();
    expect(await getCaseEgressWorkspaceId(page)).toBe(config.defaultWorkspaceId);
  });

  test("a failed disconnect shows the error page and leaves Egress connected", async ({ page }) => {
    await page.route(DISCONNECT_EGRESS_ROUTE, (route) =>
      route.request().method() === "DELETE"
        ? route.fulfill({ status: 500, body: "Internal Server Error" })
        : route.continue(),
    );

    await chooseDisconnectEgress(page);
    await answerConfirmation(page, "yes");

    await expect(page).toHaveURL(new RegExp(`/case/${caseId}/case-management/disconnect-failure\\?type=egress`));
    await expect(page.locator("h1")).toHaveText("Could not disconnect Egress");
    await expect(page.getByText("If the problem continues, contact the product team for support.")).toBeVisible();

    await page.getByRole("button", { name: "Continue" }).click();
    await expect(page).toHaveURL(new RegExp(`/case/${caseId}/case-management$`));
    expect(await getCaseEgressWorkspaceId(page)).toBe(config.defaultWorkspaceId);
  });

  test("Yes disconnects Egress and offers a link to connect again", async ({ page }) => {
    await chooseDisconnectEgress(page);
    await answerConfirmation(page, "yes");

    await expect(page).toHaveURL(new RegExp(`/case/${caseId}/case-management/disconnect-success\\?type=egress`));
    await expect(page.getByText("Egress disconnected")).toBeVisible();
    await expect(page.getByText("You've disconnected an Egress case.")).toBeVisible();
    await expect(page.getByText("You can connect to another Egress case to transfer materials.")).toBeVisible();

    expect(await getCaseEgressWorkspaceId(page), "case still has an Egress workspace").toBeNull();

    const connectLink = page.getByRole("link", { name: "Connect Egress" });
    await expect(connectLink).toHaveAttribute("href", `/search-results?urn=${caseUrn}`);
    await connectLink.click();
    await expect(page).toHaveURL(new RegExp(`/search-results\\?urn=${caseUrn}`));
  });
});
