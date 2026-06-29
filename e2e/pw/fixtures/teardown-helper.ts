import type { TestInfo } from "@playwright/test";
import {
  authenticateEgress,
  deleteFiles,
  listEgressWorkspaceFilesByFolderId,
} from "../helpers/egress-api";
import { deleteNetAppFiles } from "../helpers/netapp-api";
import { getAzureADToken } from "../helpers/auth-api";
import { loadEnvConfig } from "../helpers/env-config";
import type { UploadedFile } from "../helpers/types";

export interface TeardownContext {
  workspaceId: string;
  files: UploadedFile[];
  destinationSubfolderId?: string;
  uploadSubfolder?: string;
  destinationParentLabel: string;
  netAppFolder?: string;
  // Case id is required for the NetApp batch-delete endpoint
  // (DeleteNetAppBatch reads sourcePath against caseMetadata.NetappFolderPath).
  // Undefined skips NetApp teardown — default mode only surfaces this when
  // DEFAULT_CASE_ID is configured.
  caseId?: number;
  testInfo: TestInfo;
  // Optional pre-acquired Egress token. When omitted, a fresh one is minted
  // from config.egressServiceAccountAuth so default-mode callers don't have
  // to authenticate again before invoking teardown.
  egressToken?: string;
}

/**
 * Per-test teardown shared by default-mode and register-case fixtures.
 *
 * Runs only on test pass — on failure the uploaded files are intentionally
 * left in the dated/per-test subfolder for inspection via the Egress UI or
 * Playwright trace.
 *
 * Steps (best-effort; each section is independent):
 * 1. Delete uploaded source files from the workspace.
 * 2. If destinationSubfolderId is known, list files the LCC backend wrote
 *    into "<destinationParentLabel>/<uploadSubfolder>/" during NetApp→Egress
 *    specs and delete them. Egress→NetApp specs leave this empty (no-op).
 * 3. If netAppFolder + caseId + lccApiBaseUrl are set, mint LCC auth
 *    tokens and POST /api/v1/netapp/delete/batch to remove the NetApp-side
 *    copies that Egress→NetApp specs left behind.
 */
export async function teardownTestData(ctx: TeardownContext): Promise<void> {
  if (ctx.testInfo.status !== "passed") return;

  const config = loadEnvConfig();
  const token =
    ctx.egressToken ??
    (await authenticateEgress(
      config.egressBaseUrl,
      config.egressServiceAccountAuth
    ));

  const fileIds = ctx.files
    .map((f) => f.id)
    .filter((id): id is string => !!id);
  await deleteFiles(config.egressBaseUrl, token, ctx.workspaceId, fileIds);

  if (ctx.destinationSubfolderId) {
    const expectFile = /netapp-to-egress/i.test(ctx.testInfo.file);
    const destinationFiles = await listEgressWorkspaceFilesByFolderId(
      config.egressBaseUrl,
      token,
      ctx.workspaceId,
      ctx.destinationSubfolderId,
      expectFile
    );
    if (destinationFiles.length > 0) {
      console.log(
        `  [teardown] Deleting ${destinationFiles.length} destination file(s) from ${ctx.destinationParentLabel}/${ctx.uploadSubfolder ?? ""}/`
      );
      await deleteFiles(
        config.egressBaseUrl,
        token,
        ctx.workspaceId,
        destinationFiles.map((f) => f.id)
      );
    }
  }

  if (ctx.netAppFolder && ctx.caseId && config.lccApiBaseUrl) {
    const accessToken = await getAzureADToken(
      config.tenantId,
      config.lccApiClientId,
      config.e2eAdUser,
      config.e2eAdPassword,
    );
    await deleteNetAppFiles(
      config.lccApiBaseUrl,
      ctx.caseId,
      ctx.netAppFolder,
      ctx.files.map((f) => f.fileName),
      accessToken
    );
  }
}
