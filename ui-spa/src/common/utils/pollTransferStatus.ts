import { getTransferStatus } from "../../apis/gateway-api";
import { type TransferStatusResponse } from "../../schemas";
import { ApiError } from "../../common/errors/ApiError";

const MAX_INTERVAL_MS = 5000;
const MID_INTERVAL_MS = 3000;

/**
 * Polls the transfer status endpoint with adaptive backoff until a terminal state is reached
 * or polling should stop.
 *
 * Interval strategy:
 * - Base interval scales with transfer size: Math.min(pollingInterval + (totalFiles * 10), 5000)ms
 *   e.g. 50 files → ~1.5s, 200 files → ~3s, 400+ files → 5s cap
 * - 304 Not Modified responses trigger stepped backoff: 0–1 → base; 2–3 → 3s; 4+ → 5s.
 *   The stepped value is a floor, not an override: the interval never drops below the
 *   size-based base, so large transfers already polling at the 5s cap stay at 5s.
 * - Backoff resets on each 200 response (entity has changed)
 * - pollingInterval overrides the 1000ms default for test isolation
 */
export const pollTransferStatus = async (
  transferId: string,
  shouldStopPolling: () => boolean,
  handleResponse: (response: TransferStatusResponse) => void,
  handleError: (error: Error) => void,
  pollingInterval: number = 1000,
): Promise<void> => {
  const delay = (ms: number) =>
    new Promise<void>((resolve) => setTimeout(resolve, ms));

  let noChangeCount = 0;
  let adaptiveBase = pollingInterval;
  let lastEtag: string | null = null;

  while (!shouldStopPolling()) {
    try {
      const { data, etag } = await getTransferStatus(
        transferId,
        lastEtag ?? undefined,
      );
      lastEtag = etag;

      if (data !== null) {
        adaptiveBase = Math.min(
          pollingInterval + ((data.totalFiles ?? 0) * 10),
          MAX_INTERVAL_MS,
        );
        noChangeCount = 0;
        handleResponse(data);
        if (data.status !== "InProgress" && data.status !== "Initiated") {
          break;
        }
      } else {
        noChangeCount++;
      }
    } catch (error) {
      if (!(error instanceof ApiError && error.code === 404)) {
        handleError(error as Error);
        break;
      }
    }

    const steppedValue =
      noChangeCount >= 4
        ? MAX_INTERVAL_MS
        : noChangeCount >= 2
          ? MID_INTERVAL_MS
          : adaptiveBase;
    // Backoff is a floor: never poll faster than the size-based base interval
    await delay(Math.max(adaptiveBase, steppedValue));
  }
};
