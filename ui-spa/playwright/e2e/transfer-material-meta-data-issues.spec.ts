import { delay, HttpResponse, http } from "msw";
import { expect, test } from "../utils/test";

test.describe("egress meta data issues", () => {
  test("Should handle if the user lands on the transfer material with just missing egress connection meta data,by navigating to egress connection error page", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(1000);
        return HttpResponse.json({
          caseId: "12",
          egressWorkspaceId: "",
          netappFolderPath: "netapp/",
          operationName: "Thunderstruck",
          urn: "45AA2098221",
        });
      }),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page).toHaveURL(
      "/case/12/case-management/egress-connection-error?operation-name=Thunderstruck",
    );
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to Egress",
    );
    await expect(
      page.getByText(
        "The connection to the Egress case for Thunderstruck has stopped working.",
      ),
    ).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/",
    );
    await expect(page.getByRole("button", { name: "Reconnect" })).toBeVisible();
    await expect(page.getByRole("button", { name: "cancel" })).toBeVisible();
  });
  test("Should handle if the user lands on the transfer material with just missing egress connection and netapp connection meta data, by navigating to find a case page", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(1000);
        return HttpResponse.json({
          caseId: "12",
          egressWorkspaceId: "",
          netappFolderPath: "",
          operationName: "Thunderstruck",
          urn: "45AA2098221",
        });
      }),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.getByText("loading...")).toBeVisible();
    await expect(page).toHaveURL("/");
    await expect(page.locator("h1")).toHaveText("Find a case");
  });
  test("Should handle if the user lands on the transfer material but the egress folder is not available, by navigating to egress connection error page", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/egress/workspaces/egress_1/files",
        async () => {
          await delay(500);
          return new HttpResponse(null, { status: 404 });
        },
      ),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page).toHaveURL(
      "/case/12/case-management/egress-connection-error?operation-name=Thunderstruck",
    );
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to Egress",
    );
    await expect(
      page.getByText(
        "The connection to the Egress case for Thunderstruck has stopped working.",
      ),
    ).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Reconnect" })).toBeVisible();
    await expect(page.getByRole("button", { name: "cancel" })).toBeVisible();
    await page.getByRole("button", { name: "Reconnect" }).click();
    await expect(page).toHaveURL(
      "/case/12/egress-connect?workspace-name=Thunderstruck",
    );
    await expect(page.locator("h1")).toHaveText("Link Egress to the case");
  });
  test("Should handle if the user lands on the transfer material but the egress folder is not accessible for the user, by navigating to connection error page", async ({
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

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
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
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/",
    );
  });
  test("Should handle the case meta data api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(500);
        return HttpResponse.json(null, { status: 500 });
      }),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: API_ERROR: An error occurred contacting the server at https://mocked-out-api/api/v1/cases/12: Getting case metadata failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
  test("Should handle the get egress list api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get(
        "https://mocked-out-api/api/v1/egress/workspaces/egress_1/files",
        async () => {
          await delay(500);
          return HttpResponse.json(null, { status: 500 });
        },
      ),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");

    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: API_ERROR: An error occurred contacting the server at https://mocked-out-api/api/v1/egress/workspaces/egress_1/files?folder-id=&skip=0&take=50: Getting egress folders failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });
});

test.describe("netapp meta data issues", () => {
  test("Should handle if the user lands on the transfer material with just missing netapp connection meta data,by navigating to netapp connection error page", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/cases/12", async () => {
        await delay(1000);
        return HttpResponse.json({
          caseId: "12",
          egressWorkspaceId: "egress_1",
          netappFolderPath: "",
          operationName: "Thunderstruck",
          urn: "45AA2098221",
        });
      }),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page).toHaveURL(
      "/case/12/case-management/shared-drive-connection-error?operation-name=Thunderstruck",
    );
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to the Shared Drive",
    );
    await expect(
      page.getByText(
        "The connection to the Shared Drive folder for the Thunderstruck case has stopped working.",
      ),
    ).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/",
    );
    await expect(page.getByRole("button", { name: "Reconnect" })).toBeVisible();
    await expect(page.getByRole("button", { name: "cancel" })).toBeVisible();
  });
  test("Should handle if the user lands on the transfer material but the netapp folder is not available, by navigating to netapp connection error page", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/netapp/files", async () => {
        await delay(500);
        return new HttpResponse(null, { status: 404 });
      }),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page).toHaveURL(
      "/case/12/case-management/shared-drive-connection-error?operation-name=Thunderstruck",
    );
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to the Shared Drive",
    );
    await expect(
      page.getByText(
        "The connection to the Shared Drive folder for the Thunderstruck case has stopped working.",
      ),
    ).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Reconnect" })).toBeVisible();
    await expect(page.getByRole("button", { name: "cancel" })).toBeVisible();
    await page.getByRole("button", { name: "Reconnect" }).click();
    await expect(page).toHaveURL(
      "/case/12/netapp-connect?operation-name=Thunderstruck",
    );
    await expect(page.locator("h1")).toHaveText(
      "Link a Shared Drive folder to the case",
    );
  });
  test("Should handle if the user lands on the transfer material but the netapp folder is not accessible for the user, by navigating to connection error page", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/netapp/files", async () => {
        await delay(500);
        return new HttpResponse(null, { status: 401 });
      }),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");
    await expect(page).toHaveURL(
      "/case/12/case-management/connection-error?type=shareddrive",
    );
    await expect(page.locator("h1")).toHaveText(
      "There is a problem connecting to the Shared Drive",
    );
    const listItems = page.locator("ul > li");
    await expect(listItems).toHaveCount(2);
    await expect(listItems).toHaveText([
      "check the case exists and you have access on the Case Management System",
      "contact the product team if you need help",
    ]);
    await expect(page.getByRole("link", { name: "Back" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/",
    );
  });
  test("Should handle the get Shared Drive list api error", async ({
    page,
    worker,
  }) => {
    await worker.use(
      http.get("https://mocked-out-api/api/v1/netapp/files", async () => {
        await delay(500);
        return HttpResponse.json(null, { status: 500 });
      }),
    );

    await page.goto("/case/12/case-management");
    await expect(page).toHaveURL("/case/12/case-management");

    await expect(page.locator("h1")).toHaveText(
      "Sorry, there is a problem with the service",
    );
    await expect(
      page.getByText(
        "Please try this case again later. If the problem continues, contact the product team.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Error: API_ERROR: An error occurred contacting the server at https://mocked-out-api/api/v1/netapp/files: getting netapp files/folders failed; status - Internal Server Error (500)",
      ),
    ).toBeVisible();
  });

  test("User should not be able to land directly on the egress connection error page,it should be redirected to search case page", async ({
    page,
  }) => {
    await page.goto(
      "/case/12/case-management/egress-connection-error?operation-name=Thunderstruck",
    );
    await expect(page).toHaveURL("/");
  });

  test("User should not be able to land directly on the Shared Drive connection error page,it should be redirected to search case page", async ({
    page,
  }) => {
    await page.goto(
      "/case/12/case-management/shared-drive-connection-error?operation-name=Thunderstruck",
    );
    await expect(page).toHaveURL("/");
  });
});
