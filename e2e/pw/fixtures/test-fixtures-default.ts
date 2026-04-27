import { test as base, expect } from "@playwright/test";
import { setupDefaultTestData } from "./setup-helper-default";
import { authenticateEgress, deleteFiles } from "../helpers/egress-api";
import { loadEnvConfig } from "../helpers/env-config";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Default mode: uploads to known workspace, uses existing case (already connected)
  // Skips workspace creation, case registration, Egress/NetApp connect
  testData: async ({ page }, use, testInfo) => {
    const result = await setupDefaultTestData(page);
    await use(result);

    // Per-test teardown. Only delete uploaded files on success so a failing
    // run's artefacts stay in the dated subfolder for inspection. NEVER
    // delete the workspace itself — DEFAULT_WORKSPACE_ID is shared.
    if (testInfo.status === "passed") {
      const config = loadEnvConfig();
      const token = await authenticateEgress(
        config.egressBaseUrl,
        config.egressServiceAccountAuth
      );
      const fileIds = result.files
        .map((f) => f.id)
        .filter((id): id is string => !!id);
      await deleteFiles(
        config.egressBaseUrl,
        token,
        result.workspace.id,
        fileIds
      );
    }
  },
});

export { expect };
