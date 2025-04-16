export type NetAppFolder = {
  folderPath: string;
  caseId: number | null;
};

export type NetAppFolderData = NetAppFolder[];

export type NetAppFolderResponse = {
  data: NetAppFolderData;
  pagination: {
    totalResults: number;
    skip: number;
    take: number;
    count: number;
  };
};
