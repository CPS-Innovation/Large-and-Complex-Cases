import { EgreessToNetAppTransferPayload } from "../../schemas/requests/initiateFileTransferPayload";
import { IndexingError } from "../../schemas/responses/indexingFileTransferResponse";

export type TransferResolvePageLocationState = {
  isRouteValid: true;
  validationErrors: IndexingError[];
  destinationPath: string;
  initiateTransferPayload: EgreessToNetAppTransferPayload;
};
