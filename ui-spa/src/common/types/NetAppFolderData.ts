export type NetAppFolder = {
  folderPath: string;
};

export type NetAppFile = {
  filePath: string;
  lastModified: string;
  size: number;
};

export type NetAppFolderData = {
  folders: NetAppFolder[];
  files: NetAppFile[];
};

export type NetAppFolderResponse = {
  data: NetAppFolderData;
  pagination: {
    maxKeys: number;
    nextContinuationToken: string | null;
  };
};
