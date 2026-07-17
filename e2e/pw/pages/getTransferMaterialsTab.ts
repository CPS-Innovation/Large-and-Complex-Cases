import { Page } from "@playwright/test";
import { loadEnvConfig } from "../helpers/env-config";
import { TransferMaterialsTab } from "./TransferMaterialsTab";
import { TransferMaterialsTabV1 } from "./TransferMaterialsTabV1";
import { TransferMaterialsTabApi } from "./TransferMaterialsTabApi";

export type { TransferMaterialsTabApi };

/**
 * Build the Transfer Materials page object for the active screen — the
 * `TRANSFER_MATERIALS_V1` switch decides. Specs call this to stay
 * screen-agnostic.
 */
export function getTransferMaterialsTab(page: Page): TransferMaterialsTabApi {
  const { transferMaterialsV1 } = loadEnvConfig();
  return transferMaterialsV1
    ? new TransferMaterialsTabV1(page)
    : new TransferMaterialsTab(page);
}
