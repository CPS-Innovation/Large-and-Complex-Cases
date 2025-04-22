export type NetAppFolder = {
  folderPath: string;
  caseId: number | null;
};

export type NetAppFolderData = NetAppFolder[];

export type NetAppFolderResponse = {
  data: NetAppFolderData;
  pagination: {
    maxKeys: number;
    nextContinuationToken: string | null;
  };
};
