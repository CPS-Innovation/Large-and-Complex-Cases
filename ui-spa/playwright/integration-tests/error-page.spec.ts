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
  await expect(page.getByTestId("error-reference")).toBeVisible();
  await expect(page.getByTestId("error-reference")).toHaveText(
    `Error code: ${correlationId}`,
  );
});

test("Should show the error page with error message if any other error happens", async ({
  page,
  worker,
}) => {
  await worker.use(
    http.get("https://mocked-out-api/api/v1/areas", async () => {
      await delay(200);
      return HttpResponse.error();
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
  await expect(page.getByTestId("error-reference")).toBeVisible();
});
