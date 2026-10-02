import { test, expect, type Page, type Request } from "@playwright/test";
import { browserLogin } from "../fixtures/setup-helper";

const AREAS_ROUTE = "**/api/v1/areas";
const HEADING = "Sorry, there is a problem with the service";
const BODY = "Contact the product team and give them the error code.";

type TelemetryProperty = Record<string, unknown>;

function leakedFragments(failedUrl: string): string[] {
  const { host, pathname } = new URL(failedUrl);
  return [
    host,
    pathname,
    "API Error",
    "returned 500",
    "Internal Server Error",
    "Getting case areas failed",
    "Network Error",
    "Failed to fetch",
    "TypeError",
  ];
}

type FailedRequest = { correlationId: string; failedUrl: string };

async function failAreasRequest(
  page: Page,
  fail: "status500" | "networkError",
): Promise<{ intercepted: Promise<FailedRequest> }> {
  let resolve!: (value: FailedRequest) => void;
  const intercepted = new Promise<FailedRequest>((r) => (resolve = r));
  await page.route(AREAS_ROUTE, async (route) => {
    resolve({
      correlationId: route.request().headers()["correlation-id"] ?? "",
      failedUrl: route.request().url(),
    });
    if (fail === "status500") {
      await route.fulfill({ status: 500, body: "Internal Server Error" });
    } else {
      await route.abort("failed");
    }
  });
  return { intercepted };
}

function isExceptionTelemetry(request: Request): boolean {
  if (request.method() !== "POST" || !request.url().includes("/api/v1/telemetry")) {
    return false;
  }
  return request.postDataJSON()?.telemetryType === "Exception";
}

function telemetryProperties(request: Request): TelemetryProperty {
  const props: TelemetryProperty[] = request.postDataJSON().properties ?? [];
  return Object.assign({}, ...props);
}

async function expectSanitisedErrorPage(
  page: Page,
  correlationId: string,
  failedUrl: string,
) {
  await expect(page.locator("h1")).toHaveText(HEADING);
  await expect(page.getByText(BODY)).toBeVisible();
  await expect(page.getByTestId("error-reference")).toHaveText(
    `Error code: ${correlationId}`,
  );

  const pageText = await page.locator("body").innerText();
  for (const fragment of leakedFragments(failedUrl)) {
    expect(pageText, `error page leaks "${fragment}"`).not.toContain(fragment);
  }
  expect(pageText, "error page shows a stack trace").not.toMatch(/\n\s+at \S+/);
}

test.describe("Global error boundary (FCT2-22112)", () => {
  test.beforeEach(async ({ page }) => {
    test.setTimeout(180_000);
    await browserLogin(page);
  });

  for (const fail of ["status500", "networkError"] as const) {
    test(`shows a sanitised error page with the correlation ID when /api/v1/areas fails (${fail})`, async ({
      page,
    }) => {
      const { intercepted } = await failAreasRequest(page, fail);
      const telemetryRequest = page.waitForRequest(isExceptionTelemetry);
      const telemetryResponse = page.waitForResponse((r) =>
        isExceptionTelemetry(r.request()),
      );

      await page.goto(process.env.BASE_URL!);

      const { correlationId, failedUrl } = await intercepted;
      expect(correlationId, "app sent a Correlation-Id on /api/v1/areas").toMatch(
        /^[0-9a-f-]{36}$/i,
      );

      await test.step("error page is the GOV.UK pattern, sanitised", async () => {
        await expectSanitisedErrorPage(page, correlationId, failedUrl);
      });

      await test.step("error is reported to POST /api/v1/telemetry", async () => {
        const props = telemetryProperties(await telemetryRequest);
        expect(props).toMatchObject({
          referenceId: correlationId,
          errorSource: "API_ERROR",
          errorName: "API_ERROR",
          route: "/",
        });
        expect(props.exceptionMessage).toEqual(expect.any(String));
        expect(props.errorStack).toEqual(expect.any(String));

        const response = await telemetryResponse;
        expect(response.ok(), `telemetry returned ${response.status()}`).toBeTruthy();
      });
    });
  }
});
