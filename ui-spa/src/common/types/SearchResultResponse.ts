export type SearchResult = {
  operationName: string;
  urn: string;
  caseId: number;
  leadDefendantName: string;
  egressWorkspaceId: string | null;
  netappFolderPath: string | null;
  registrationDate: string | null;
};

export type SearchResultData = SearchResult[];
