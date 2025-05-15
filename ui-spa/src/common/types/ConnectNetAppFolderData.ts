export type ConnectNetAppFolder = {
  path: string;
  caseId: number | null;
};

export type ConnectNetAppFolderData = {
  rootPath: string;
  folders: ConnectNetAppFolder[];
};

export type ConnectNetAppFolderResponse = {
  data: ConnectNetAppFolderData;
  pagination: {
    maxKeys: number;
    nextContinuationToken: string | null;
  };
};
