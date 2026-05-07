import { test as base, expect } from "@playwright/test";
import * as fs from "fs";
import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  createFolder,
  deleteFiles,
  listEgressWorkspaceFilesByFolderId,
  uploadFile,
} from "../helpers/egress-api";
import { deleteNetAppFile } from "../helpers/netapp-api";
import { getAuthTokens } from "../helpers/auth-api";
import { REGISTER_CASE_NETAPP_FOLDER } from "../helpers/constants";
import type { TestSetupResult, UploadedFile } from "../helpers/types";
import {
  STATE_FILE,
  type RegisterCaseSharedState,
} from "../helpers/register-case-state";
import { browserLogin } from "./setup-helper";

export interface RegisterCaseTestOptions {
  fileSizeMb: number;
  fileCount: number;
}

const SOURCE_PARENT = "4. Served Evidence";
const DESTINATION_PARENT = "2. Counsel only";

function slugify(title: string): string {
  return title
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "")
    .slice(0, 40);
}

export const test = base.extend<
  { testOptions: RegisterCaseTestOptions; testData: TestSetupResult }
>({
  testOptions: [{ fileSizeMb: 100, fileCount: 1 }, { option: true }],

  testData: async ({ testOptions, page }, use, testInfo) => {
    if (!fs.existsSync(STATE_FILE)) {
      throw new Error(
        `Shared register-case state missing at ${STATE_FILE}. ` +
          "Ensure the register-case-setup project ran first."
      );
    }
    const shared: RegisterCaseSharedState = JSON.parse(
      fs.readFileSync(STATE_FILE, "utf-8")
    );

    const config = loadEnvConfig();
    const token = await authenticateEgress(
      config.egressBaseUrl,
      config.egressServiceAccountAuth
    );

    // Per-test subfolder isolates each spec's file list and destination
    // from its siblings inside the shared workspace. Same pattern used in
    // default mode; see README "Cleanup / Test Data Hygiene".
    const rand = Math.floor(Math.random() * 10_000);
    const uploadSubfolder = `e2e-${slugify(testInfo.title)}-${rand}`;
    const uploadPath = `${SOURCE_PARENT}/${uploadSubfolder}/`;

    console.log(
      `  Ensuring per-test subfolder ${uploadSubfolder} exists in source + destination...`
    );
    await createFolder(
      config.egressBaseUrl,
      token,
      shared.workspace.id,
      SOURCE_PARENT,
      uploadSubfolder
    );
    // Capture destination folder id for per-test teardown of any file the
    // LCC backend wrote there during NetApp->Egress copy specs.
    const destinationSubfolderId = await createFolder(
      config.egressBaseUrl,
      token,
      shared.workspace.id,
      DESTINATION_PARENT,
      uploadSubfolder
    );

    console.log(
      `  Uploading ${testOptions.fileCount} x ${testOptions.fileSizeMb}MB file(s) to ${uploadPath}...`
    );
    const fileSizeBytes = testOptions.fileSizeMb * 1024 * 1024;
    const files: UploadedFile[] = [];
    for (let i = 1; i <= testOptions.fileCount; i++) {
      const timestamp = new Date()
        .toISOString()
        .replace(/[:.]/g, "-")
        .slice(0, 19);
      const fileName = `generated-${testOptions.fileSizeMb}MB-${timestamp}-file${i}.txt`;
      const file = await uploadFile(
        config.egressBaseUrl,
        token,
        shared.workspace.id,
        fileSizeBytes,
        fileName,
        uploadPath
      );
      files.push(file);
    }

    // Refresh the tactical + AD session per test and wait for the search
    // radios to be enabled before handing control to the spec. This mirrors
    // the manual flow and avoids HTTP 400 on /api/v1/case-search when
    // tactical cookies saved by the setup project have aged.
    await browserLogin(page);

    await use({
      workspace: shared.workspace,
      caseUrn: shared.caseUrn,
      files,
      uploadSubfolder,
    });

    // Per-test teardown. Only delete on success — on failure we leave the
    // uploaded files in the dated subfolder so they can be inspected in the
    // Egress UI or via the Playwright trace. The shared workspace itself is
    // torn down by the register-case-teardown project at end of run.
    if (testInfo.status === "passed") {
      const fileIds = files.map((f) => f.id).filter((id): id is string => !!id);
      await deleteFiles(
        config.egressBaseUrl,
        token,
        shared.workspace.id,
        fileIds
      );

      // Egress destination cleanup. NetApp->Egress specs copy a file into
      // "2. Counsel only/<uploadSubfolder>/"; the destination basename is
      // chosen by the LCC backend, so we list by folder id and delete
      // rather than predicting the name. Egress->NetApp specs don't
      // write to this folder so the list comes back empty and the call
      // is a no-op. Skipped when the folder id is unknown (rare —
      // implies the per-test slug collided at setup time).
      if (destinationSubfolderId) {
        const expectFile = /netapp-to-egress/i.test(testInfo.file);
        const destinationFiles = await listEgressWorkspaceFilesByFolderId(
          config.egressBaseUrl,
          token,
          shared.workspace.id,
          destinationSubfolderId,
          expectFile
        );
        if (destinationFiles.length > 0) {
          console.log(
            `  [teardown] Deleting ${destinationFiles.length} destination file(s) from ${DESTINATION_PARENT}/${uploadSubfolder}/`
          );
          await deleteFiles(
            config.egressBaseUrl,
            token,
            shared.workspace.id,
            destinationFiles.map((f) => f.id)
          );
        }
      }

      // NetApp side cleanup. Egress->NetApp specs leave a copy at
      // <REGISTER_CASE_NETAPP_FOLDER>/<fileName> (basename only — see
      // deleteNetAppFile docs for why the relativePath is empty here).
      // NetApp->Egress specs don't push there, so the DELETE 404s and is
      // warned. Skipped if LCC_API_BASE_URL unset (e.g. prod run where
      // the endpoint 403s anyway). Note: register-case mode uses a
      // different NetApp folder than default mode, so we don't read
      // NETAPP_OPERATION_NAME from env here.
      if (config.lccApiBaseUrl) {
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
        for (const file of files) {
          await deleteNetAppFile(
            config.lccApiBaseUrl,
            REGISTER_CASE_NETAPP_FOLDER,
            file.fileName,
            accessToken,
            cmsAuth
          );
        }
      }
    }
  },
});

export { expect };
