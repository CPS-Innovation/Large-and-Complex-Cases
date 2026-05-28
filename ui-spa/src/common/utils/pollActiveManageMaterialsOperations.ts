import { getActiveManageMaterialsOperations } from "../../apis/gateway-api";
import { type ManageMaterialsActiveResponse } from "../../schemas";

const POLL_INTERVAL_MS = 10_000;

/**
 * Polls the active manage materials operations endpoint on a fixed interval
 * while the tab is active, so locked checkboxes become enabled as soon as
 * another user's operation completes.
 *
 * The first call is delayed by one interval — callers should perform an
 * immediate fetch before starting this loop.
 *
 * handleResponse returns true to continue polling, false to stop.
 * Returning false when the ops list is empty avoids unnecessary polling
 * once all active operations have cleared.
 */
export const pollActiveManageMaterialsOperations = async (
  caseId: string,
  shouldStopPolling: () => boolean,
  handleResponse: (ops: ManageMaterialsActiveResponse) => boolean,
  pollingInterval: number = POLL_INTERVAL_MS,
): Promise<void> => {
  const delay = (ms: number) =>
    new Promise<void>((resolve) => setTimeout(resolve, ms));

  while (!shouldStopPolling()) {
    await delay(pollingInterval);

    if (shouldStopPolling()) break;

    try {
      const ops = await getActiveManageMaterialsOperations(caseId);
      if (!shouldStopPolling()) {
        const shouldContinue = handleResponse(ops);
        if (!shouldContinue) break;
      }
    } catch {
      // Swallow — banner is informational, polling will retry next interval
    }
  }
};
