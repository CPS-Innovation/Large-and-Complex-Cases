import { loadEnvConfig } from "../helpers/env-config";
import {
  listEgressWorkspaceFilesByFolderId,
  authenticateEgress,
} from "./egress-api";
import { getAzureADToken } from "./auth-api";
import { expect } from "@playwright/test";

// These verify helpers run once per file. Fetching a fresh token on every call
// hammers the auth endpoints — over a long soak run the AAD ROPC endpoint
// returns transient 400s (throttling). Cache each token at module scope and
// reuse it; the AAD token is refreshed once on a 401 so a run that outlives the
// token still recovers.
let cachedAadToken: string | undefined;
let cachedEgressToken: string | undefined;

async function getVerifyAadToken(
  config: ReturnType<typeof loadEnvConfig>,
  forceRefresh = false,
): Promise<string> {
  if (forceRefresh || !cachedAadToken) {
    cachedAadToken = await getAzureADToken(
      config.tenantId,
      config.lccApiClientId,
      config.e2eAdUser,
      config.e2eAdPassword,
    );
  }
  return cachedAadToken;
}

async function getVerifyEgressToken(
  config: ReturnType<typeof loadEnvConfig>,
): Promise<string> {
  if (!cachedEgressToken) {
    cachedEgressToken = await authenticateEgress(
      config.egressBaseUrl,
      config.egressServiceAccountAuth,
    );
  }
  return cachedEgressToken;
}

export async function verifyNetAppFileSizeByName(
  filePath: string,
  caseId: number,
  expectedSizeBytes: number,
  accessToken?: string | undefined,
): Promise<void> {
  const config = loadEnvConfig();
  const url = `${config.lccApiBaseUrl}/api/v1/netapp/search?case-id=${caseId}&query=${encodeURIComponent(filePath)}`;
  const search = (token: string) =>
    fetch(url, {
      method: "GET",
      headers: { Authorization: `Bearer ${token}` },
    });

  // Use the caller's token if provided; otherwise the shared cached one.
  let response = await search(accessToken ?? (await getVerifyAadToken(config)));

  // A cached token can expire on a long run — refresh once on 401 and retry.
  if (response.status === 401 && !accessToken) {
    response = await search(await getVerifyAadToken(config, true));
  }

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `NetApp search failed (${response.status}) for '${filePath}': ${text.slice(0, 200)}`,
    );
  }

  const data = (await response.json()).data;
  let file: {
    key: string;
    type: "File" | "Folder";
    size: number;
    lastModified: string;
  };

  switch (data.length) {
    case 0:
      throw new Error(`No match found for '${filePath}'`);
    case 1:
      file = data[0];
      break;
    default:
      throw new Error(
        `Search response must not match more than a single file.`,
      );
  }

  const folderPrefix = config.netAppOperationName.endsWith("/")
    ? config.netAppOperationName
    : `${config.netAppOperationName}/`;

  const fullPath = `${folderPrefix}${filePath}`;

  if (file.key !== fullPath) {
    throw new Error(
      `The file path returned does not match '${fullPath}'.\n` +
        `Returned: '${file.key}'.`,
    );
  }

  expect(file.size, `NetApp file '${filePath}' has unexpected size`).toBe(
    expectedSizeBytes,
  );
}

export async function isFileInEgress(
  workspaceId: string,
  folderId: string,
  fileName: string,
  egressToken?: string | undefined,
): Promise<boolean> {
  const config = loadEnvConfig();

  const files = await listEgressWorkspaceFilesByFolderId(
    config.egressBaseUrl,
    egressToken ?? (await getVerifyEgressToken(config)),
    workspaceId,
    folderId,
  );

  return files.some((f) => f.fileName === fileName);
}

// Poll the Egress folder listing (API, no browser) until the file appears.
// A large file is present via the uploads endpoint before Egress finishes
// processing it into the workspace listing the UI reads, so gate the UI wait
// on this cheap API check rather than burning the browser timeout reloading a
// panel that can't show the file yet.
export async function waitForFileInEgress(
  workspaceId: string,
  folderId: string,
  fileName: string,
  options: {
    timeoutMs?: number;
    pollIntervalMs?: number;
    egressToken?: string;
  } = {},
): Promise<void> {
  const {
    timeoutMs = 15 * 60 * 1000,
    pollIntervalMs = 10_000,
    egressToken,
  } = options;
  const start = Date.now();

  for (;;) {
    if (await isFileInEgress(workspaceId, folderId, fileName, egressToken)) {
      return;
    }
    if (Date.now() - start > timeoutMs) {
      throw new Error(
        `Timed out after ${Math.round(timeoutMs / 1000)}s waiting for '${fileName}' ` +
          `to appear in the Egress folder listing (folder id: ${folderId}).`,
      );
    }
    await new Promise((r) => setTimeout(r, pollIntervalMs));
  }
}
