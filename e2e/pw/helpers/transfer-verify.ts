import { loadEnvConfig } from "../helpers/env-config";
import { listEgressWorkspaceFilesByFolderId, authenticateEgress } from "./egress-api";
import { getAzureADToken } from "./auth-api";
import { expect } from "@playwright/test";

const config =loadEnvConfig()

export async function verifyNetAppFileSizeByName(
  filePath: string,
  caseId: number,
  expectedSizeBytes: number,
  netAppOperationName: string = config.netAppOperationName,
  accessToken?: string | undefined,
): Promise<void> {
  if (!accessToken) {
    accessToken = await getAzureADToken(
      config.tenantId,
      config.lccApiClientId,
      config.e2eAdUser,
      config.e2eAdPassword,
    );
  }

  const response = await fetch(
    `${config.lccApiBaseUrl}/api/v1/netapp/search?case-id=${caseId}&query=${encodeURIComponent(filePath)}`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${accessToken}`,
      }
    }
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `NetApp search failed (${response.status}) for '${filePath}': ${text.slice(0, 200)}`
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
      throw new Error(`Search response must not match more than a single file.`);
  }

  const folderPrefix = netAppOperationName.endsWith("/")
    ? netAppOperationName
    : `${netAppOperationName}/`;

  const fullPath = `${folderPrefix}${filePath}`;

  if (file.key !== fullPath) {
    throw new Error(
      `The file path returned does not match '${fullPath}'.\n` +
      `Returned: '${file.key}'.`
    );
  }

  expect(
    file.size,
    `NetApp file '${filePath}' has unexpected size`
  ).toBe(expectedSizeBytes);
}

export async function isFileInEgress(
  workspaceId: string,
  folderId: string,
  fileName: string,
  egressToken?: string | undefined,
): Promise<boolean> {
  if (!egressToken) {
    egressToken = await authenticateEgress(
      config.egressBaseUrl,
      config.egressServiceAccountAuth,
    )
  }

  const files = await listEgressWorkspaceFilesByFolderId(
    config.egressBaseUrl,
    egressToken,
    workspaceId,
    folderId,
    true,
  );

  return files.some(f => f.fileName === fileName);
}