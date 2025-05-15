export type NetAppFolder = {
  path: string;
};

export type NetAppFile = {
  path: string;
  lastModified: string;
  filesize: number;
};

export type NetAppFolderResponse = {
  data: NetAppFolderDataResponse;
  pagination: {
    maxKeys: number;
    nextContinuationToken: string | null;
  };
};

export type NetAppFolderDataResponse = {
  fileData: NetAppFile[];
  folderData: NetAppFolder[];
};

export type NetAppFolderData = {
  path: string;
  lastModified: string;
  filesize: number;
  isFolder: boolean;
}[];
