import { getTransferStatus } from "../../apis/gateway-api";
import { TransferStatusResponse } from "../types/TransferStatusResponse";
import { ApiError } from "../../common/errors/ApiError";

export const pollTransferStatus = async (
  transferId: string,
  stopPolling: boolean,
  handleResponse: (response: TransferStatusResponse) => void,
  handleError: (error: Error) => void,
) => {
  const delay = (ms: number) => {
    return new Promise((resolve) => setTimeout(resolve, ms));
  };

  while (!stopPolling) {
    try {
      const response = await getTransferStatus(transferId);
      handleResponse(response);

      if (response.status !== "InProgress" && response.status !== "Initiated") {
        break;
      }
    } catch (error) {
      if ((error as ApiError).code !== 404) handleError(error as Error);
    }
    await delay(1000);
  }
};
