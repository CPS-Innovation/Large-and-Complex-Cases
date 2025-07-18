export type IndexingError = {
  id: string;
  sourcePath: string;
};

export type IndexingFileTransferResponse = {
  caseId: number;
  isInvalid: boolean;
  destinationPath: string;
  validationErrors: IndexingError[];
  files: { id?: string; sourcePath: string; relativePath?: string; fullFilePath?: string }[];
};
