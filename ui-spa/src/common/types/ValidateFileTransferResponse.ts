export type ValidateFileTransferResponse = {
  caseId: string;
  isValid: boolean;
  destinationBasePath: string;
  errors: {
    id: string;
    sourcePath: string;
    errorType: string;
    message: string;
  }[];
  discoveredFiles: { id: string; sourcePath: string }[];
};
