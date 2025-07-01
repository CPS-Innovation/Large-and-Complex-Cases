export type IndexingFileTransferResponse = {
  caseId: number;
  isInvalid: boolean;
  destinationPath: string;
  validationErrors: {
    id: string;
    sourcePath: string;
    errorType: string;
    message: string;
  }[];
  files: { id?: string; sourcePath: string, relativePath?: string }[];
};
