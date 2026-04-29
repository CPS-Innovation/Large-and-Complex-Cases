import * as dotenv from "dotenv";
import * as path from "path";

dotenv.config({
  path: path.resolve(__dirname, `../.env.${process.env.ENVIRONMENT || "local"}`),
});

import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  findNextWorkspaceName,
  createWorkspace,
  addUserToWorkspace,
  uploadFile,
} from "../helpers/egress-api";

// Standalone create-workspace-and-upload script. No browser, no case
// registration. Creates a fresh AUTOMATION-TESTING* workspace, adds the
// E2E user, and uploads N x M MB files into "4. Served Evidence/".
// File count + size come from TEST_FILE_COUNT / TEST_FILE_SIZE_MB.

async function main() {
  const config = loadEnvConfig();
  const fileSizeMb = config.testFileSizeMb;
  const fileCount = config.testFileCount;

  console.log("Authenticating with Egress...");
  const token = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );

  console.log("Finding next workspace name...");
  const workspaceName = await findNextWorkspaceName(
    config.egressBaseUrl,
    token
  );
  console.log(`  Workspace: ${workspaceName}`);

  console.log("Creating workspace...");
  const workspaceId = await createWorkspace(
    config.egressBaseUrl,
    token,
    workspaceName,
    config.egressTemplateId
  );
  console.log(`  Workspace id: ${workspaceId}`);

  console.log(`Adding user ${config.e2eAdUser}...`);
  await addUserToWorkspace(
    config.egressBaseUrl,
    token,
    workspaceId,
    config.e2eAdUser,
    config.egressAdminRoleId
  );

  const uploadPath = "4. Served Evidence/";
  console.log(
    `Uploading ${fileCount} x ${fileSizeMb}MB file(s) to ${uploadPath}...`
  );
  const fileSizeBytes = fileSizeMb * 1024 * 1024;
  const uploaded: { id?: string; fileName: string }[] = [];
  for (let i = 1; i <= fileCount; i++) {
    const timestamp = new Date()
      .toISOString()
      .replace(/[:.]/g, "-")
      .slice(0, 19);
    const fileName = `generated-${fileSizeMb}MB-${timestamp}-file${i}.txt`;
    const file = await uploadFile(
      config.egressBaseUrl,
      token,
      workspaceId,
      fileSizeBytes,
      fileName,
      uploadPath
    );
    uploaded.push({ id: file.id, fileName: file.fileName });
  }

  console.log("\n=== Done ===");
  console.log(`Workspace: ${workspaceName} (${workspaceId})`);
  console.log("Files:");
  for (const f of uploaded) {
    console.log(`  ${f.fileName}  (id: ${f.id ?? "<none>"})`);
  }
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
