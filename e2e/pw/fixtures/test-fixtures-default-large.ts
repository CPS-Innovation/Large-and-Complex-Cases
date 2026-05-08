import { test as base, expect } from "@playwright/test";
import { setupDefaultTestData } from "./setup-helper-default";
import { teardownTestData } from "./teardown-helper";
import { loadEnvConfig } from "../helpers/env-config";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Default mode with large file: 1 x 200MB, uses existing case
  testData: [async ({ page }, use, testInfo) => {
    const result = await setupDefaultTestData(page, {
      fileSizeMb: 200,
      fileCount: 1,
    });
    await use(result);

    // Per-test teardown mirrors test-fixtures-default.ts. Only delete on
    // success so a failing run's large file stays in the dated subfolder
    // for inspection. NEVER delete the shared workspace.
    const config = loadEnvConfig();
    await teardownTestData({
      workspaceId: result.workspace.id,
      files: result.files,
      destinationSubfolderId: result.destinationSubfolderId,
      uploadSubfolder: result.uploadSubfolder,
      destinationParentLabel: "2. Counsel only",
      netAppFolder: config.netAppOperationName,
      testInfo,
    });
  }, { timeout: 300_000 }],
});

export { expect };
