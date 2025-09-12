export type IndexingError = {
  id: string;
  sourcePath: string;
};

export type IndexingFileTransferResponse = {
  caseId: number;
  isInvalid: boolean;
  destinationPath: string;
  validationErrors: IndexingError[];
  sourceRootFolderPath: string;
  files: {
    id?: string;
    sourcePath: string;
    relativePath?: string;
    fullFilePath?: string;
  }[];
};
