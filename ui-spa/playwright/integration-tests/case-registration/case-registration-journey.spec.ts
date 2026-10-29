import { test } from "../utils/test";
import { delay, HttpResponse, http } from "msw";
import { CaseMaterialsManageEgressPage } from "../pages/case-registration-manage-egress";
import { CaseRegistrationEgressConnectedConfirmationPage } from "../pages/case-registration-egress-connected-confirmation";
import { EgressConnectExistingCasePage } from "../pages/egress-connect-existing-case";
import { CaseRegistrationEgressConnectFailurePage } from "../pages/case-registration-egress-connect-failure";
import { CASE_REGISTRATION_JOURNEY_URN } from "../../../src/mocks/handlers";

test("case registration create new egress flow", async ({ page }) => {
  await page.goto(
    `/case-registration/manage-set-up-egress?urn=${CASE_REGISTRATION_JOURNEY_URN}`,
  );
  const caseMaterialsManageEgressPage = new CaseMaterialsManageEgressPage(page);
  await caseMaterialsManageEgressPage.verifyUrl(
    `/case-registration/manage-set-up-egress?urn=${CASE_REGISTRATION_JOURNEY_URN}`,
  );
  await caseMaterialsManageEgressPage.verifyPageElements();
  await caseMaterialsManageEgressPage.errorValidations();
  await caseMaterialsManageEgressPage.selectCreateNewEgress();
  await caseMaterialsManageEgressPage.clickContinue();

  const caseRegistrationEgressConnectedConfirmationPage =
    new CaseRegistrationEgressConnectedConfirmationPage(page);
  await caseRegistrationEgressConnectedConfirmationPage.verifyUrl(
    "/case-registration/egress-connect-confirmation",
  );
  await caseRegistrationEgressConnectedConfirmationPage.verifyPageElements();
});

test("case registration connect existing egress case flow", async ({
  page,
  worker,
}) => {
  await worker.use(
    http.post("https://mocked-out-api/api/v1/egress/connections", async () => {
      await delay(100);
      return new HttpResponse(null, { status: 200 });
    }),
  );
  await page.goto(
    `/case-registration/manage-set-up-egress?urn=${CASE_REGISTRATION_JOURNEY_URN}`,
  );
  const caseMaterialsManageEgressPage = new CaseMaterialsManageEgressPage(page);
  await caseMaterialsManageEgressPage.verifyUrl(
    `/case-registration/manage-set-up-egress?urn=${CASE_REGISTRATION_JOURNEY_URN}`,
  );
  await caseMaterialsManageEgressPage.verifyPageElements();
  await caseMaterialsManageEgressPage.errorValidations();
  await caseMaterialsManageEgressPage.selectConnectExistingEgress();
  await caseMaterialsManageEgressPage.clickContinue();

  const egressConnectExistingCasePage = new EgressConnectExistingCasePage(page);
  await egressConnectExistingCasePage.verifyUrl(
    "/case-registration/egress-connect-search-results?workspace-name=Thunderstruck1_pl",
  );
  await egressConnectExistingCasePage.verifyPageElements("Thunderstruck1_pl");
  await egressConnectExistingCasePage.verifySearchResult(
    "Thunderstruck1_pl",
    2,
  );
  await egressConnectExistingCasePage.verifyTableHeaders([
    "Action",
    "Name",
    "Status",
  ]);
  await egressConnectExistingCasePage.verifyTableRows([
    ["Connect", "thunderstrike", "Available to connect"],
    ["", "thunderstrike1", "Connected to Thunderstrike1 Case"],
  ]);
  await egressConnectExistingCasePage.clickConnectBtn(0);
  await egressConnectExistingCasePage.verifyAllButtonsDisabled();

  const caseRegistrationEgressConnectedConfirmationPage =
    new CaseRegistrationEgressConnectedConfirmationPage(page);
  await caseRegistrationEgressConnectedConfirmationPage.verifyUrl(
    "http://localhost:5173/case-registration/egress-connect-confirmation?egress-workspace-name=thunderstrike",
  );
  await caseRegistrationEgressConnectedConfirmationPage.verifyPageElements(
    "thunderstrike",
  );
});

test("case registration connect existing egress case error flow", async ({
  page,
  worker,
}) => {
  await worker.use(
    http.post("https://mocked-out-api/api/v1/egress/connections", async () => {
      await delay(100);
      return new HttpResponse(null, { status: 500 });
    }),
  );
  await page.goto(
    `/case-registration/manage-set-up-egress?urn=${CASE_REGISTRATION_JOURNEY_URN}`,
  );
  const caseMaterialsManageEgressPage = new CaseMaterialsManageEgressPage(page);
  await caseMaterialsManageEgressPage.verifyUrl(
    `/case-registration/manage-set-up-egress?urn=${CASE_REGISTRATION_JOURNEY_URN}`,
  );
  await caseMaterialsManageEgressPage.verifyPageElements();
  await caseMaterialsManageEgressPage.errorValidations();
  await caseMaterialsManageEgressPage.selectConnectExistingEgress();
  await caseMaterialsManageEgressPage.clickContinue();

  const egressConnectExistingCasePage = new EgressConnectExistingCasePage(page);
  await egressConnectExistingCasePage.verifyUrl(
    "/case-registration/egress-connect-search-results?workspace-name=Thunderstruck1_pl",
  );
  await egressConnectExistingCasePage.verifyPageElements("Thunderstruck1_pl");
  await egressConnectExistingCasePage.verifySearchResult(
    "Thunderstruck1_pl",
    2,
  );
  await egressConnectExistingCasePage.verifyTableHeaders([
    "Action",
    "Name",
    "Status",
  ]);
  await egressConnectExistingCasePage.verifyTableRows([
    ["Connect", "thunderstrike", "Available to connect"],
    ["", "thunderstrike1", "Connected to Thunderstrike1 Case"],
  ]);
  await egressConnectExistingCasePage.clickConnectBtn(0);
  const caseRegistrationEgressConnectFailurePage =
    new CaseRegistrationEgressConnectFailurePage(page);
  await caseRegistrationEgressConnectFailurePage.verifyUrl();
  await caseRegistrationEgressConnectFailurePage.verifyPageElements();
  await caseRegistrationEgressConnectFailurePage.verifyBackLink(
    "/case-registration/egress-connect-search-results?workspace-name=Thunderstruck1_pl",
  );
  await caseRegistrationEgressConnectFailurePage.tryAgainBtnClick();
  await egressConnectExistingCasePage.verifyUrl(
    "/case-registration/egress-connect-search-results?workspace-name=Thunderstruck1_pl",
  );
  await egressConnectExistingCasePage.verifyPageElements("Thunderstruck1_pl");
});
