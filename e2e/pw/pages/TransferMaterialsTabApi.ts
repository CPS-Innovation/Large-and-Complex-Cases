/**
 * Screen-agnostic contract for the Transfer Materials tab, implemented by both
 * the old-screen page object (`TransferMaterialsTab`) and the new-screen one
 * (`TransferMaterialsTabV1`). Specs construct via `getTransferMaterialsTab`
 * and depend only on this surface, so a spec never references a
 * screen-specific selector.
 *
 * The surface is exactly the set of methods the specs call today. New-screen
 * work fills in the v1 implementation behind these same signatures.
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
  selectAction(action: "Copy" | "Move"): Promise<void>;
  selectReverseAction(action: "Copy" | "Move"): Promise<void>;
  confirmTransfer(): Promise<void>;
  waitForTransferComplete(timeout?: number): Promise<void>;
  /**
   * Recover from the new-screen transfer error page back to case
   * management, so the Transfer Materials tab can be re-entered. No-op on
   * the old screen (which keeps the transfer view mounted through errors).
   */
  dismissTransferErrorIfPresent(): Promise<void>;
  /**
   * Assert the named file is present in the NetApp / shared-drive panel.
   * On the old screen both panels are visible so it checks the panel in
   * place; on the new screen only the source panel renders, so it switches
   * to the shared drive first. Throws if the file never appears.
   */
  verifyNetAppContainsFile(fileName: string, timeout?: number): Promise<void>;
  navigateToFolder(folderName: string): Promise<void>;
  waitForEgressFileByName(
    fileName: string,
    folderPath: string[],
    timeout?: number,
  ): Promise<void>;
}
