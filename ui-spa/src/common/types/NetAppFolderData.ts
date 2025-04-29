export type NetAppFolder = {
  folderPath: string;
  caseId: number | null;
};

export type NetAppFolderData = {
  rootPath: string;
  folders: NetAppFolder[];
};

export type NetAppFolderResponse = {
  data: NetAppFolderData;
  pagination: {
    maxKeys: number;
    nextContinuationToken: string | null;
  };
};
