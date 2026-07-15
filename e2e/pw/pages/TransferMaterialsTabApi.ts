/**
 * Screen-agnostic Transfer Materials contract, implemented by both
 * `TransferMaterialsTab` (old) and `TransferMaterialsTabV1` (new). Specs build
 * via `getTransferMaterialsTab` and depend only on this surface, never a
 * screen-specific selector.
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
   * Initiate a transfer in the given direction (default Egress → NetApp).
   * `direction` only matters on the old screen, which has a Copy/Move control in
   * each panel's inset; the new screen has a single shared control and ignores
   * it (direction is implied by the current source).
   */
  selectAction(
    action: "Copy" | "Move",
    direction?: "egressToNetApp" | "netAppToEgress",
  ): Promise<void>;
  /** Confirm the pending transfer. `action` must match the Copy/Move just
   * initiated — the new screen's confirm button reads "<action> to <folder>". */
  confirmTransfer(action: "Copy" | "Move"): Promise<void>;
  waitForTransferComplete(timeout?: number): Promise<void>;
  /** Recover from a transfer error page back to case management so the tab can
   * be re-entered. No-op when not on an error page. */
  dismissTransferErrorIfPresent(): Promise<void>;
  /** Assert the named file is present in the NetApp / shared-drive panel
   * (old screen checks in place; new screen switches to the shared drive
   * first). Throws if it never appears. */
  verifyNetAppContainsFile(fileName: string, timeout?: number): Promise<void>;
  navigateToFolder(folderName: string): Promise<void>;
  waitForEgressFileByName(
    fileName: string,
    folderPath: string[],
    timeout?: number,
  ): Promise<void>;
}
