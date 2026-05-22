export type EgressConnectConfirmationRouteState = {
  isRouteValid: boolean;
  backLinkUrl: string;
  caseId: string;
  searchQueryString: string;
  isNetAppConnected: boolean;
  selectedWorkspace: {
    id: string;
    name: string;
  };
};
