import { delay, HttpResponse, http } from "msw";
import { expect, test } from "./utils/test";

test("Should show the error page with correlation id if api fails", async ({
  page,
  worker,
}) => {
  let correlationId = "";
  await worker.use(
    http.get("https://mocked-out-api/api/v1/areas", async (req: any) => {
      await delay(200);
      correlationId = req.request.headers?.get?.("correlation-id") ?? "";

      return new HttpResponse(null, { status: 500 });
    }),
  );

  await page.goto("/");
  await expect(page.getByText("Loading...")).toBeVisible();
  await expect(page.getByText("Loading...")).not.toBeVisible();
  await expect(page.locator("h1")).toHaveText(
    "Sorry, there is a problem with the service",
  );

  await expect(
    page.getByText("Contact the product team and give them the error code."),
  ).toBeVisible();
  await expect(page.getByTestId("txt-error-correlation-id")).toBeVisible();
  await expect(page.getByTestId("txt-error-correlation-id")).toHaveText(
    `Error code: ${correlationId}`,
  );
  await expect(page.getByTestId("txt-error-message")).not.toBeVisible();
});

test("Should show the error page with error message if any other error happens", async ({
  page,
  worker,
}) => {
  await worker.use(
    http.get("https://mocked-out-api/api/v1/areas", async (req: any) => {
      await delay(200);
      //this way we try to access unavailable property so that worker will fail to return a response which is caught
      return req.headers.abc;
    }),
  );

  await page.goto("/");
  await expect(page.getByText("Loading...")).toBeVisible();
  await expect(page.getByText("Loading...")).not.toBeVisible();
  await expect(page.locator("h1")).toHaveText(
    "Sorry, there is a problem with the service",
  );

  await expect(
    page.getByText("Contact the product team and give them the error code."),
  ).toBeVisible();
  await expect(page.getByTestId("txt-error-message")).toBeVisible();
  await expect(page.getByTestId("txt-error-message")).toHaveText(
    `Error code: Failed to fetch`,
  );
  await expect(page.getByTestId("txt-error-correlation-id")).not.toBeVisible();
});
