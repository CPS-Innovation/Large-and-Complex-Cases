import { EgreessToNetAppTransferPayload } from "../types/InitiateFileTransferPayload";
import { IndexingError } from "../types/IndexingFileTransferResponse";

export type TransferResolvePageLocationState = {
  isRouteValid: true;
  validationErrors: IndexingError[];
  destinationPath: string;
  initiateTransferPayload: EgreessToNetAppTransferPayload;
  baseFolderName: string;
};
