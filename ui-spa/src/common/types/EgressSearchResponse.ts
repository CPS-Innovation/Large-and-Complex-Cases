export type EgressSearchResult = {
  dateCreated: string;
  id: string;
  name: string;
};

export type EgressSearchResultData = {
  data: EgressSearchResult[];
  pagination: {
    totalResults: number;
    skip: number;
    take: number;
    count: number;
  };
};
