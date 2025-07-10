import { getTransferStatus } from "../../apis/gateway-api";
import { TransferStatusResponse } from "../types/TransferStatusResponse";
import { ApiError } from "../../common/errors/ApiError";

/**
 * Polls the transfer status endpoint at regular intervals until a terminal state is reached or polling should stop
 * @param transferId - The ID of the transfer to check status for
 * @param shouldStopPolling - Function that determines if polling should be stopped
 * @param handleResponse - Callback function to process each status response
 * @param handleError - Callback function to handle any errors during polling
 * @param pollingInterval - Time in milliseconds between poll attempts (default: 1000ms)
 */
export const pollTransferStatus = async (
  transferId: string,
  shouldStopPolling: () => boolean,
  handleResponse: (response: TransferStatusResponse) => void,
  handleError: (error: Error) => void,
  pollingInterval: number = 1000,
): Promise<void> => {
  const delay = (ms: number) => {
    return new Promise((resolve) => setTimeout(resolve, ms));
  };

  while (!shouldStopPolling()) {
    try {
      const response = await getTransferStatus(transferId);
      handleResponse(response);
      if (response.status !== "InProgress" && response.status !== "Initiated") {
        break;
      }
    } catch (error) {
      if (!(error instanceof ApiError && error.code === 404)) {
        handleError(error as Error);
        break;
      }
    }
    await delay(pollingInterval);
  }
};
