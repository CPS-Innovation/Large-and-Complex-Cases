export type SearchResult = {
  operationName: string;
  urn: string;
  leadDefendantName: string;
  egressStatus: "connected" | "pending";
  sharedDriveStatus: "connected" | "pending";
  registrationDate: string;
};

export type SearchResultData = SearchResult[];
