import { EgreessToNetAppTransferPayload } from "../../schemas/requests/initiateFileTransferPayload";
import { type IndexingError } from "../../schemas";

export type TransferResolvePageLocationState = {
  isRouteValid: true;
  validationErrors: IndexingError[];
  destinationPath: string;
  initiateTransferPayload: EgreessToNetAppTransferPayload;
};
