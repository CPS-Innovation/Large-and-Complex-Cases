import { Page } from "@playwright/test";
import { loadEnvConfig } from "../helpers/env-config";
import { TransferMaterialsTab } from "./TransferMaterialsTab";
import { TransferMaterialsTabV1 } from "./TransferMaterialsTabV1";
import { TransferMaterialsTabApi } from "./TransferMaterialsTabApi";

export type { TransferMaterialsTabApi };

/**
 * Construct the Transfer Materials page object for the screen the target
 * environment is running. Specs call this instead of `new TransferMaterialsTab`
 * so they stay agnostic of which screen is active; the `TRANSFER_MATERIALS_V1`
 * switch (see helpers/env-config.ts) decides.
 */
export function getTransferMaterialsTab(page: Page): TransferMaterialsTabApi {
  const { transferMaterialsV1 } = loadEnvConfig();
  return transferMaterialsV1
    ? new TransferMaterialsTabV1(page)
    : new TransferMaterialsTab(page);
}
