import { test as base, expect } from "@playwright/test";
import { setupDefaultTestData } from "./setup-helper-default";
import { authenticateEgress, deleteFiles } from "../helpers/egress-api";
import { deleteNetAppFile } from "../helpers/netapp-api";
import { getAuthTokens } from "../helpers/auth-api";
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

      // NetApp side cleanup: Egress->NetApp specs leave a copy at
      // <NETAPP_OPERATION_NAME>/<fileName>. NetApp->Egress specs don't
      // push there, so the DELETE 404s and is silently warned. Skipped
      // entirely if LCC_API_BASE_URL or NETAPP_OPERATION_NAME unset
      // (e.g. CI configured against prod, where the endpoint 403s anyway).
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
