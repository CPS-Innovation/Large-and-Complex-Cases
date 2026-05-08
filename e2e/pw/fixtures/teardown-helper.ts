import type { TestInfo } from "@playwright/test";
import {
  authenticateEgress,
  deleteFiles,
  listEgressWorkspaceFilesByFolderId,
} from "../helpers/egress-api";
import { deleteNetAppFile } from "../helpers/netapp-api";
import { getAuthTokens } from "../helpers/auth-api";
import { loadEnvConfig } from "../helpers/env-config";
import type { UploadedFile } from "../helpers/types";

export interface TeardownContext {
  workspaceId: string;
  files: UploadedFile[];
  destinationSubfolderId?: string;
  uploadSubfolder?: string;
  destinationParentLabel: string;
  netAppFolder?: string;
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
 * 3. If netAppFolder + lccApiBaseUrl are set, mint LCC auth tokens and
 *    delete the NetApp-side copies that Egress→NetApp specs left behind.
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

  if (ctx.netAppFolder && config.lccApiBaseUrl) {
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
    for (const file of ctx.files) {
      await deleteNetAppFile(
        config.lccApiBaseUrl,
        ctx.netAppFolder,
        file.fileName,
        accessToken,
        cmsAuth
      );
    }
  }
}
