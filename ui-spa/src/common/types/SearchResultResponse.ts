export type SearchResult = {
  operationName: string;
  urn: string;
  caseId: number;
  leadDefendantName: string;
  egressStatus: "connected" | "inactive";
  sharedDriveStatus: "connected" | "inactive";
  registrationDate: string;
};

export type SearchResultData = SearchResult[];
