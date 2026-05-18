import { test as base, expect } from "@playwright/test";
import { setupDefaultTestData } from "./setup-helper-default";
import { teardownTestData } from "./teardown-helper";
import { loadEnvConfig } from "../helpers/env-config";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Default mode: uploads to known workspace, uses existing case (already connected)
  // Skips workspace creation, case registration, Egress/NetApp connect.
  // Fixture timeout bumped to 5 min to cover the 100MB upload + tactical
  // + AAD login + radio-buttons retry path on slow-tactical days; the
  // project-level 120s default isn't enough.
  testData: [async ({ page }, use, testInfo) => {
    const result = await setupDefaultTestData(page);
    await use(result);

    // Per-test teardown. Only delete uploaded files on success so a failing
    // run's artefacts stay in the dated subfolder for inspection. NEVER
    // delete the workspace itself — DEFAULT_WORKSPACE_ID is shared.
    const config = loadEnvConfig();
    await teardownTestData({
      workspaceId: result.workspace.id,
      files: result.files,
      destinationSubfolderId: result.destinationSubfolderId,
      uploadSubfolder: result.uploadSubfolder,
      destinationParentLabel: "2. Counsel only",
      netAppFolder: config.netAppOperationName,
      caseId: result.caseId,
      testInfo,
    });
  }, { timeout: 300_000 }],
});

export { expect };
