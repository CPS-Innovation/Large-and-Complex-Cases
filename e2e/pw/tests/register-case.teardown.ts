import { test as teardown } from "@playwright/test";
import * as fs from "fs";
import { loadEnvConfig } from "../helpers/env-config";
import {
  authenticateEgress,
  deleteWorkspace,
  listAutomationWorkspaces,
} from "../helpers/egress-api";
import { disassociateNetAppConnection } from "../helpers/netapp-api";
import { getAzureADAppToken } from "../helpers/auth-api";
import {
  AUTH_FILE,
  STATE_FILE,
  type RegisterCaseSharedState,
} from "../helpers/register-case-state";

// Rolling 24h sweep. Runs via Playwright's `teardown:` hook on the
// register-case-setup project. Deletes AUTOMATION-TESTING* workspaces that
// are older than 24h, keeping today's run alive for post-mortem inspection
// and always preserving DEFAULT_WORKSPACE_ID. Per-test file cleanup is
// handled by the fixture regardless of this sweep.

const MAX_AGE_MS = 24 * 60 * 60 * 1000;

teardown("sweep workspaces older than 24 hours", async () => {
  teardown.setTimeout(180_000);

  const config = loadEnvConfig();
  const token = await authenticateEgress(
    config.egressBaseUrl,
    config.egressServiceAccountAuth
  );

  const keepIds = new Set<string>();
  if (config.defaultWorkspaceId) {
    keepIds.add(config.defaultWorkspaceId);
  }
  let currentRunCaseId: number | undefined;
  if (fs.existsSync(STATE_FILE)) {
    const shared: RegisterCaseSharedState = JSON.parse(
      fs.readFileSync(STATE_FILE, "utf-8")
    );
    keepIds.add(shared.workspace.id);
    currentRunCaseId = shared.caseId;
  }

  // Disassociate the current run's case from its NetApp connection
  // before sweeping. Endpoint requires an app-only AAD token (client-
  // credentials flow against the LCC API app registration) rather than
  // the user-delegated tokens used elsewhere in the suite.
  if (
    currentRunCaseId !== undefined &&
    config.lccApiBaseUrl &&
    config.lccApiClientId &&
    config.lccApiClientSecret
  ) {
    console.log(
      `  Disassociating NetApp connection for case ${currentRunCaseId}...`
    );
    const appToken = await getAzureADAppToken(
      config.tenantId,
      config.lccApiClientId,
      config.lccApiClientSecret
    );
    await disassociateNetAppConnection(
      config.lccApiBaseUrl,
      currentRunCaseId,
      appToken
    );
  }

  const cutoff = Date.now() - MAX_AGE_MS;
  // Listing failures throw — we don't want a silent "no stale workspaces"
  // result when the API is actually broken. Individual deleteWorkspace
  // calls below stay best-effort (warn + continue) because a single
  // sticky workspace shouldn't fail the whole teardown.
  const all = await listAutomationWorkspaces(config.egressBaseUrl, token);
  const stale = all.filter(
    (w) => !keepIds.has(w.id) && new Date(w.dateCreated).getTime() < cutoff
  );

  console.log(
    `  Found ${all.length} AUTOMATION-TESTING* workspaces total; ${stale.length} older than 24h and eligible for deletion.`
  );
  console.log(`  Preserving (current + DEFAULT): ${[...keepIds].join(", ") || "<none>"}`);

  for (const ws of stale) {
    console.log(
      `  Deleting ${ws.name} (${ws.id}, created ${ws.dateCreated})...`
    );
    await deleteWorkspace(config.egressBaseUrl, token, ws.id);
  }

  // Clear per-run state so the next run creates its own fresh workspace and
  // state file. The workspace itself is kept live by the rolling sweep.
  fs.rmSync(STATE_FILE, { force: true });
  fs.rmSync(AUTH_FILE, { force: true });
});
