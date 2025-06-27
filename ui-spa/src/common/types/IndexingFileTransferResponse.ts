export type IndexingError = {
  id: string;
  sourcePath: string;
  errorType: string;
};

export type IndexingFileTransferResponse = {
  caseId: number;
  isInvalid: boolean;
  destinationPath: string;
  validationErrors: IndexingError[];
  files: { id?: string; sourcePath: string }[];
};
