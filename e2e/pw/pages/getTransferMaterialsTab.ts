import { Page } from "@playwright/test";
import { TransferMaterialsTabV1 } from "./TransferMaterialsTabV1";
import { TransferMaterialsTabApi } from "./TransferMaterialsTabApi";

export type { TransferMaterialsTabApi };

/**
 * Build the Transfer Materials page object. The old (v0) screen was removed in
 * FCT2-21686, so there is a single implementation; specs keep calling this so
 * they depend on `TransferMaterialsTabApi` rather than a concrete class.
 */
export function getTransferMaterialsTab(page: Page): TransferMaterialsTabApi {
  return new TransferMaterialsTabV1(page);
}
