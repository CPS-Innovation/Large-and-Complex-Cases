import { test as setup } from "@playwright/test";
import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  createFolder,
  uploadFile,
  deleteFiles,
} from "../helpers/egress-api";
import { browserLogin } from "../fixtures/setup-helper";
import { CaseSearchPage } from "../pages/CaseSearchPage";
import { SearchResultsPage } from "../pages/SearchResultsPage";
import { CaseManagementPage } from "../pages/CaseManagementPage";
import { TransferMaterialsTab } from "../pages/TransferMaterialsTab";
import { NETAPP_FIXTURE_FILENAME } from "../helpers/constants";

// One-shot seed: puts the canonical NetApp source fixture at
// `<NETAPP_OPERATION_NAME>/lcc-e2e-fixture-source.txt` for the default-mode
// NetApp -> Egress spec to consume.
//
// Opt-in: skipped unless RUN_SEED=1 so the unfiltered `npx playwright
// test` invocation produces a clean report without a duplicate-file
// rejection on every run. Seed once per environment via:
//   RUN_SEED=1 npx playwright test --project=seed-netapp-fixture
//
// Browser-based because /api/v1/filetransfer/initiate's CmsAuthValuesAuth
// attribute is stricter than /cases/.../netapp/delete and rejects the
// header-based token pair we mint in helpers/auth-api.ts; a real browser
// session has the full tactical cookie set the endpoint expects.

const SEED_PARENT = "4. Served Evidence";
const SEED_SUBFOLDER = "lcc-e2e-fixture-seed";
const SEED_SIZE_MB = 1;

setup("seed lcc-e2e-fixture-source.txt to NetApp", async ({ page }) => {
  setup.skip(
    !process.env.RUN_SEED,
    "Opt-in seed — run with RUN_SEED=1 npx playwright test --project=seed-netapp-fixture"
  );
  setup.setTimeout(300_000);

  const config = loadEnvConfig();
  if (!config.defaultWorkspaceId) {
    throw new Error("DEFAULT_WORKSPACE_ID is required");
  }
  if (!config.defaultCaseUrn) {
    throw new Error("DEFAULT_CASE_URN is required");
  }

  console.log("Authenticating with Egress + uploading seed source...");
  const egressToken = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );
  await createFolder(
    config.egressBaseUrl,
    egressToken,
    config.defaultWorkspaceId,
    SEED_PARENT,
    SEED_SUBFOLDER
  );
  const uploaded = await uploadFile(
    config.egressBaseUrl,
    egressToken,
    config.defaultWorkspaceId,
    SEED_SIZE_MB * 1024 * 1024,
    NETAPP_FIXTURE_FILENAME,
    `${SEED_PARENT}/${SEED_SUBFOLDER}/`
  );

  console.log("Logging in via browser...");
  await browserLogin(page);

  console.log(`Searching for case ${config.defaultCaseUrn}...`);
  const caseSearch = new CaseSearchPage(page);
  await caseSearch.searchByUrn(config.defaultCaseUrn);

  const searchResults = new SearchResultsPage(page);
  await searchResults.waitForResults();
  await searchResults.clickCaseAction(config.defaultCaseUrn);

  const caseMgmt = new CaseManagementPage(page);
  await caseMgmt.waitForLoad();
  await caseMgmt.switchToTab("transfer-materials");

  const transferTab = new TransferMaterialsTab(page);
  await transferTab.waitForEgressFiles();
  await transferTab.navigateToFolder(SEED_PARENT);
  await transferTab.waitForEgressFiles();
  await transferTab.navigateToFolder(SEED_SUBFOLDER);
  await transferTab.waitForEgressFiles();

  await transferTab.waitForEgressFileByName(NETAPP_FIXTURE_FILENAME, [
    SEED_PARENT,
    SEED_SUBFOLDER,
  ]);
  await transferTab.selectEgressFileByName(NETAPP_FIXTURE_FILENAME);
  await transferTab.selectAction("Copy");
  await transferTab.confirmTransfer();
  await transferTab.waitForTransferComplete();

  console.log("Deleting Egress-side seed source...");
  if (uploaded.id) {
    await deleteFiles(
      config.egressBaseUrl,
      egressToken,
      config.defaultWorkspaceId,
      [uploaded.id]
    );
  }

  console.log(
    `\n=== Done ===\nFixture seeded at: <NetApp>/${NETAPP_FIXTURE_FILENAME}`
  );
});
