import { getTransferStatus } from "../../apis/gateway-api";
import { type TransferStatusResponse } from "../../schemas";
import { ApiError } from "../../common/errors/ApiError";

const MAX_INTERVAL_MS = 5000;
const MID_INTERVAL_MS = 3000;

/**
 * How long (wall-clock) we tolerate consecutive 404s before giving up. A 404
 * means the Durable entity TransferEntityState/{transferId} does not exist yet.
 * This is expected for a short window between InitiateTransfer returning
 * Accepted and the orchestrator creating the entity, so we allow a grace
 * window. Past this we treat the transfer as missing and surface an error
 * instead of polling forever (e.g. an orchestration that failed before the
 * entity was created, or a stale/incorrect transfer ID).
 */
const NOT_FOUND_GRACE_MS = 30000;

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
 *
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
  let firstNotFoundAt: number | null = null;
  let adaptiveBase = pollingInterval;
  let lastEtag: string | null = null;

  while (!shouldStopPolling()) {
    try {
      const { data, etag } = await getTransferStatus(
        transferId,
        lastEtag ?? undefined,
      );
      lastEtag = etag;

      // A non-throwing response (200 or 304) means the entity exists, so the
      // create race is over: reset the 404 grace window.
      firstNotFoundAt = null;

      if (data !== null) {
        adaptiveBase = Math.min(
          pollingInterval + (data.totalFiles ?? 0) * 10,
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

      firstNotFoundAt ??= Date.now();
      if (Date.now() - firstNotFoundAt >= NOT_FOUND_GRACE_MS) {
        handleError(
          new ApiError(
            "Transfer not found",
            `${transferId}/status`,
            { status: 404, statusText: "Not Found" },
            undefined,
            "The transfer could not be found. It may have failed to start or the transfer ID is no longer valid.",
          ),
        );
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
