export type EgressFolder = {
  id: string;
  name: string;
  isFolder: boolean;
  dateUpdated: string;
  path: string;
};

export type EgressFolderData = EgressFolder[];

export type EgressFolderResponse = {
  data: EgressFolderData;
  pagination: {
    totalResults: number;
    skip: number;
    take: number;
    count: number;
  };
};
