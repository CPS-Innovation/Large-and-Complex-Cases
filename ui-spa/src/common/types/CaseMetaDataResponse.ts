export type CaseMetaDataResponse = {
  caseId: string;
  egressWorkspaceId: string;
  netappFolderPath: string;
  operationName: string;
  activeTransferId: string | null;
  urn: string;
};
