import { test as base, expect } from "@playwright/test";
import { setupDefaultTestData } from "./setup-helper-default";
import { authenticateEgress, deleteFiles } from "../helpers/egress-api";
import { deleteNetAppFile } from "../helpers/netapp-api";
import { getAuthTokens } from "../helpers/auth-api";
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

      // NetApp side cleanup — see test-fixtures-default.ts for details.
      if (config.lccApiBaseUrl && config.netAppOperationName) {
        const { accessToken, cmsAuth } = await getAuthTokens(
          config.tenantId,
          config.clientId,
          config.e2eAdUser,
          config.e2eAdPassword,
          config.ddeiBaseUrl,
          config.ddeiAccessKey,
          config.cmsUsername,
          config.cmsPassword
        );
        for (const file of result.files) {
          await deleteNetAppFile(
            config.lccApiBaseUrl,
            config.netAppOperationName,
            file.fileName,
            accessToken,
            cmsAuth
          );
        }
      }
    }
  }, { timeout: 300_000 }],
});

export { expect };
