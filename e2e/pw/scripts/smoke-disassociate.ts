import * as dotenv from "dotenv";
import * as path from "path";

dotenv.config({
  path: path.resolve(__dirname, `../.env.${process.env.ENVIRONMENT || "local"}`),
});

import { loadEnvConfig } from "../helpers/env-config";
import { getAzureADAppToken } from "../helpers/auth-api";

// Smoke + diagnostic helper. Mints a real AAD token via our auth path,
// performs three DELETE attempts against /api/v1/netapp/connections with
// different auth shapes, and prints a copy-pasteable PowerShell snippet
// that uses the SAME token so you can re-test bearer-only behaviour from
// a real PS session.
//
// Uses a non-existent case id so it never affects live data.

const FAKE_CASE_ID = 999999999;

async function main() {
  const config = loadEnvConfig();
  if (!config.lccApiBaseUrl) throw new Error("LCC_API_BASE_URL is required");

  if (!config.lccApiClientId || !config.lccApiClientSecret) {
    throw new Error("LCC_API_CLIENT_ID and LCC_API_CLIENT_SECRET are required");
  }

  console.log("=== Minting AAD app-only token (client-credentials) ===");
  const accessToken = await getAzureADAppToken(
    config.tenantId,
    config.lccApiClientId,
    config.lccApiClientSecret
  );
  console.log(`accessToken length: ${accessToken.length}`);

  const url = `${config.lccApiBaseUrl}/api/v1/netapp/connections?case-id=${FAKE_CASE_ID}`;
  console.log(`\n=== DELETE ${url} ===`);
  const r = await fetch(url, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  console.log(`Status: ${r.status} ${r.statusText}`);
  console.log(`Body:   ${(await r.text()).slice(0, 300)}`);

  console.log("\n=== Verdict ===");
  if (r.status === 401) {
    console.log("FAIL: app token rejected — check client_id/secret and app role assignment.");
    process.exit(1);
  } else if (r.status === 200 || r.status === 204) {
    console.log("WARN: 2xx on a fake case id — backend may have happily 'disassociated' nothing.");
  } else {
    console.log(`OK: non-401 means auth was accepted (${r.status} expected for non-existent case).`);
  }
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
