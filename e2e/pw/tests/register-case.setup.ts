import { test as setup } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";
import { setupTestData } from "../fixtures/setup-helper";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { EgressConnectPage } from "../pages/EgressConnectPage";
import { EgressConfirmationPage } from "../pages/EgressConfirmationPage";
import { NetAppConnectPage } from "../pages/NetAppConnectPage";
import { NetAppConfirmationPage } from "../pages/NetAppConfirmationPage";
import {
  AUTH_FILE,
  STATE_FILE,
  type RegisterCaseSharedState,
} from "../helpers/register-case-state";

setup("register and connect shared case", async ({ page }) => {
  setup.setTimeout(600_000);

  // Full API setup + browser login. Uploads a 1MB seed file so Egress has
  // content when the UI renders the connected workspace; per-test files are
  // uploaded later by individual specs.
  const { workspace, caseUrn, caseId } = await setupTestData(page, {
    fileSizeMb: 1,
    fileCount: 1,
  });

  console.log(`=== Searching for case URN: ${caseUrn} ===`);
  const caseSearch = new CaseSearchPage(page);
  await caseSearch.selectUrnSearch();
  await caseSearch.fillUrn(caseUrn);
  await caseSearch.clickSearch();

  const searchResults = new SearchResultsPage(page);
  await searchResults.waitForResults();
  await searchResults.clickCaseAction(caseUrn);

  console.log(`=== Connecting Egress workspace: ${workspace.name} ===`);
  const egressConnect = new EgressConnectPage(page);
  await egressConnect.searchFolder(workspace.name);
  await egressConnect.waitForResults();
  await egressConnect.connectFolder();

  const egressConfirm = new EgressConfirmationPage(page);
  await egressConfirm.confirmConnect();

  console.log("=== Connecting NetApp folder ===");
  const netappConnect = new NetAppConnectPage(page);
  await netappConnect.waitForFolders();
  await netappConnect.connectFolder();

  const netappConfirm = new NetAppConfirmationPage(page);
  await netappConfirm.confirmConnect();

  console.log("=== Persisting shared state for register-case specs ===");
  fs.mkdirSync(path.dirname(AUTH_FILE), { recursive: true });
  fs.mkdirSync(path.dirname(STATE_FILE), { recursive: true });

  await page.context().storageState({ path: AUTH_FILE });

  const shared: RegisterCaseSharedState = { workspace, caseUrn, caseId };
  fs.writeFileSync(STATE_FILE, JSON.stringify(shared, null, 2));
});
