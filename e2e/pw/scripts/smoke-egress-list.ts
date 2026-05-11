import * as dotenv from "dotenv";
import * as path from "path";

dotenv.config({
  path: path.resolve(__dirname, `../.env.${process.env.ENVIRONMENT || "local"}`),
});

import { loadEnvConfig } from "../helpers/env-config";
import { authenticateEgress } from "../helpers/egress-api";

// Diagnostic: probes the Egress /workspaces/<id>/files endpoint with
// different filter shapes against the default-mode workspace. Run with:
//   npx tsx scripts/smoke-egress-list.ts <folder_id>
// where <folder_id> is a known-good folder (e.g. one of the 4 top-level
// folders, or a recently-created subfolder).

async function main() {
  const folderId = process.argv[2];
  const config = loadEnvConfig();
  const token = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );
  const workspaceId = config.defaultWorkspaceId;
  console.log(`Workspace ${workspaceId}`);

  // 1. Root listing
  console.log("\n[1] root (no filter)");
  await probe(`/api/v1/workspaces/${workspaceId}/files?view=full&skip=0&limit=100`, token, config.egressBaseUrl);

  if (!folderId) {
    console.log("\n(no folderId arg — done)");
    return;
  }

  // 2. ?folder=<id>
  console.log(`\n[2] ?folder=${folderId}`);
  await probe(`/api/v1/workspaces/${workspaceId}/files?view=full&skip=0&limit=100&folder=${folderId}`, token, config.egressBaseUrl);

  // 3. ?folder_id=<id>
  console.log(`\n[3] ?folder_id=${folderId}`);
  await probe(`/api/v1/workspaces/${workspaceId}/files?view=full&skip=0&limit=100&folder_id=${folderId}`, token, config.egressBaseUrl);

  // 4. ?parent=<id>
  console.log(`\n[4] ?parent=${folderId}`);
  await probe(`/api/v1/workspaces/${workspaceId}/files?view=full&skip=0&limit=100&parent=${folderId}`, token, config.egressBaseUrl);
}

async function probe(relativeUrl: string, token: string, baseUrl: string) {
  console.log(`  GET ${baseUrl}${relativeUrl}`);
  const resp = await fetch(`${baseUrl}${relativeUrl}`, {
    headers: { Authorization: `Basic ${token}` },
  });
  console.log(`  -> ${resp.status}`);
  if (!resp.ok) {
    console.log(`  body: ${await resp.text()}`);
    return;
  }
  const body: any = await resp.json();
  const items = body?.data ?? [];
  console.log(`  ${items.length} item(s):`);
  for (const item of items.slice(0, 10)) {
    console.log(`    - ${item.is_folder ? "folder" : "file"} id=${item.id} name=${item.filename} path=${item.path}`);
  }
  if (items.length > 10) console.log(`    ... +${items.length - 10} more`);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
