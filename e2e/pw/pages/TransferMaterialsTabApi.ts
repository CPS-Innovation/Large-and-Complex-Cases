import type { Response } from "@playwright/test";

/**
 * Transfer Materials contract, implemented by `TransferMaterialsTabV1`. Specs
 * build via `getTransferMaterialsTab` and depend only on this surface, never a
 * concrete page object's selectors.
 */
export interface TransferMaterialsTabApi {
  waitForEgressFiles(): Promise<void>;
  waitForNetAppFiles(): Promise<void>;
  switchToNetAppSource(): Promise<void>;
  selectAllEgressFiles(): Promise<void>;
  selectNetAppFiles(indices: number[]): Promise<void>;
  selectNetAppFileByExactName(fileName: string): Promise<void>;
  /** Sort the shared-drive/NetApp panel by last-modified date descending. */
  sortNetAppByDateDescending(): Promise<void>;
  selectEgressFileByName(fileName: string): Promise<void>;
  /**
   * Initiate a Copy or Move of the selected files. The screen has a single
   * shared Copy/Move control; the direction is set by the current view
   * (see `switchToNetAppSource`), not by this call.
   */
  selectAction(action: "Copy" | "Move"): Promise<void>;
  /** Confirm the pending transfer. `action` must match the Copy/Move just
   * initiated — the confirm button reads "<action> to <folder>". */
  confirmTransfer(action: "Copy" | "Move"): Promise<void>;
  waitForTransferComplete(timeout?: number): Promise<void>;
  /** Call before `waitForTransferComplete`. */
  watchForTransferClear(timeout?: number): Promise<Response | null>;
  verifySuccessBannerClearedOnReload(
    clearSettled?: Promise<Response | null>,
  ): Promise<void>;
  /** Recover from a transfer error page back to case management so the tab can
   * be re-entered. No-op when not on an error page. */
  dismissTransferErrorIfPresent(): Promise<void>;
  /** Assert the named file is present in the NetApp / shared-drive panel
   * (switches to the shared drive first). Throws if it never appears. */
  verifyNetAppContainsFile(fileName: string, timeout?: number): Promise<void>;
  navigateToFolder(folderName: string): Promise<void>;
  waitForEgressFileByName(
    fileName: string,
    folderPath: string[],
    timeout?: number,
  ): Promise<void>;
}
