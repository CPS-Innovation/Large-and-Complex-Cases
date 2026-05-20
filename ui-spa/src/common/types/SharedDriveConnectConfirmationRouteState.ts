export type SharedDriveConnectConfirmationRouteState = {
  isRouteValid: boolean;
  operationName: string;
  caseId: string;
  searchQueryString: string;
  netappRootFolderPath: string;
  backLinkUrl: string;
  selectedWorkspace: {
    folderPath: string;
  };
};
