import { test as base, expect } from "@playwright/test";
import * as fs from "fs";
import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  createFolder,
  deleteFiles,
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
    await createFolder(
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

      // NetApp side cleanup. Egress->NetApp specs leave a copy at
      // <REGISTER_CASE_NETAPP_FOLDER>/<fileName>; NetApp->Egress specs
      // don't, so they 404 and are warned. Skipped if LCC_API_BASE_URL
      // unset (e.g. prod run where the endpoint 403s anyway). Note:
      // register-case mode uses a different NetApp folder than default
      // mode, so we don't read NETAPP_OPERATION_NAME from env here.
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
