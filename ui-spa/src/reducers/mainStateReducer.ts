import {
  type CaseDivisionsOrAreaResponse,
  type CaseMetaDataResponse,
} from "../schemas";
import { mapAreaLookups } from "./utils/mapAreaLookups";
import { FeatureFlagData } from "../common/types/FeatureFlagData";
import { InitiateFileTransferPayload } from "../schemas/requests/initiateFileTransferPayload";
export type FormData = {
  searchType: "urn" | "operation name" | "defendant name";
  operationName: string;
  operationArea: string;
  defendantArea: string;
  defendantName: string;
  urn: string;
};

export type MainState = {
  appData: {
    featureFlags: FeatureFlagData | null;
    connectEgressPage: {
      searchQueryString: string;
      isNetAppConnected: boolean;
    };
    connectSharedDrivePage: {
      searchQueryString: string;
      netappRootFolderPath: string;
    };
    egressConnectConfirmationPage: {
      backLinkUrl: string;
      searchQueryString: string;
      isNetAppConnected: boolean;
      selectedWorkspace: {
        id: string;
        name: string;
      };
    };
    connectSharedDriveConfirmationPage: {
      operationName: string;
      searchQueryString: string;
      netappRootFolderPath: string;
      backLinkUrl: string;
      selectedWorkspace: {
        folderPath: string;
      };
    };
    connectEgressFailurePage: {
      backLinkUrl: string;
    };
    connectSharedDriveFailurePage: {
      backLinkUrl: string;
    };
    transferDestinationPage: {
      transferSource: "egress" | "netapp" | null;
      selectedTransferAction: "copy" | "move" | null;
      sourcePaths:
        | {
            fileId: string;
            path: string;
            isFolder: boolean;
          }[]
        | { path: string }[];
      egressWorkspaceId: string;
      netAppPath: string;
      operationName: string;
    };
    transferResolveFilePathPage: {
      validationErrors: {
        id: string;
        sourcePath: string;
      }[];
      destinationPath: string;
      initiateTransferPayload: InitiateFileTransferPayload | null;
      baseFolderName: string;
    };
  };
  formData: FormData;
  apiData: {
    caseDivisionsOrAreas: CaseDivisionsOrAreaResponse | null;
    caseMetaData: CaseMetaDataResponse | null;
  };
};

export const initialState: MainState = {
  appData: {
    featureFlags: null,
    connectEgressPage: {
      searchQueryString: "",
      isNetAppConnected: false,
    },
    connectSharedDrivePage: {
      searchQueryString: "",
      netappRootFolderPath: "",
    },
    egressConnectConfirmationPage: {
      backLinkUrl: "",
      searchQueryString: "",
      isNetAppConnected: false,
      selectedWorkspace: {
        id: "",
        name: "",
      },
    },
    connectSharedDriveConfirmationPage: {
      operationName: "",
      searchQueryString: "",
      netappRootFolderPath: "",
      backLinkUrl: "",
      selectedWorkspace: {
        folderPath: "",
      },
    },
    connectEgressFailurePage: {
      backLinkUrl: "",
    },
    connectSharedDriveFailurePage: {
      backLinkUrl: "",
    },
    transferDestinationPage: {
      transferSource: null,
      selectedTransferAction: null,
      sourcePaths: [],
      egressWorkspaceId: "",
      netAppPath: "",
      operationName: "",
    },
    transferResolveFilePathPage: {
      validationErrors: [],
      destinationPath: "",
      initiateTransferPayload: null,
      baseFolderName: "",
    },
  },
  formData: {
    searchType: "urn",
    operationName: "",
    operationArea: "",
    defendantArea: "",
    defendantName: "",
    urn: "",
  },
  apiData: {
    caseDivisionsOrAreas: null,
    caseMetaData: null,
  },
};

export type MainStateActions =
  | {
      type: "SET_FORM_DATA_FIELD";
      payload: Partial<FormData>;
    }
  | {
      type: "SET_CASE_DIVISIONS_OR_AREAS";
      payload: {
        caseDivisionsOrAreas: CaseDivisionsOrAreaResponse;
      };
    }
  | {
      type: "SET_CASE_META_DATA";
      payload: {
        caseMetaData: CaseMetaDataResponse;
      };
    }
  | {
      type: "SET_FEATURE_FLAGS";
      payload: {
        featureFlags: FeatureFlagData;
      };
    }
  | {
      type: "SET_EGRESS_CONNECT_PAGE";
      payload: {
        searchQueryString: string;
        isNetAppConnected: boolean;
      };
    }
  | {
      type: "SET_SHARED_DRIVE_CONNECT_PAGE";
      payload: {
        searchQueryString: string;
        netappRootFolderPath: string;
      };
    }
  | {
      type: "SET_EGRESS_CONNECT_CONFIRMATION_PAGE";
      payload: {
        backLinkUrl: string;
        searchQueryString: string;
        isNetAppConnected: boolean;
        selectedWorkspace: {
          id: string;
          name: string;
        };
      };
    }
  | {
      type: "SET_SHARED_DRIVE_CONNECT_CONFIRMATION_PAGE";
      payload: {
        operationName: string;
        searchQueryString: string;
        netappRootFolderPath: string;
        backLinkUrl: string;
        selectedWorkspace: {
          folderPath: string;
        };
      };
    }
  | {
      type: "SET_EGRESS_CONNECT_FAILURE_PAGE";
      payload: {
        backLinkUrl: string;
      };
    }
  | {
      type: "SET_SHARED_DRIVE_CONNECT_FAILURE_PAGE";
      payload: {
        backLinkUrl: string;
      };
    }
  | {
      type: "SET_TRANSFER_DESTINATION_PAGE";
      payload: {
        transferSource: "egress" | "netapp" | null;
        selectedTransferAction: "copy" | "move" | null;
        sourcePaths:
          | {
              fileId: string;
              path: string;
              isFolder: boolean;
            }[]
          | { path: string }[];
        egressWorkspaceId: string;
        netAppPath: string;
        operationName: string;
      };
    }
  | {
      type: "SET_TRANSFER_RESOLVE_FILE_PATH_PAGE";
      payload: {
        validationErrors: {
          id: string;
          sourcePath: string;
        }[];
        destinationPath: string;
        initiateTransferPayload: InitiateFileTransferPayload | null;
        baseFolderName: string;
      };
    };

export type DispatchType = React.Dispatch<MainStateActions>;

export const mainStateReducer = (
  state: MainState,
  action: MainStateActions,
): MainState => {
  switch (action.type) {
    case "SET_CASE_DIVISIONS_OR_AREAS": {
      return {
        ...state,
        apiData: {
          ...state.apiData,
          caseDivisionsOrAreas: mapAreaLookups(
            action.payload.caseDivisionsOrAreas,
          ),
        },
      };
    }
    case "SET_CASE_META_DATA": {
      return {
        ...state,
        apiData: {
          ...state.apiData,
          caseMetaData: action.payload.caseMetaData,
        },
      };
    }

    case "SET_FEATURE_FLAGS": {
      return {
        ...state,
        appData: {
          ...state.appData,
          featureFlags: action.payload.featureFlags,
        },
      };
    }
    case "SET_FORM_DATA_FIELD": {
      return {
        ...state,
        formData: {
          ...state.formData,
          ...action.payload,
        },
      };
    }

    case "SET_EGRESS_CONNECT_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          connectEgressPage: {
            ...action.payload,
          },
        },
      };
    }
    case "SET_SHARED_DRIVE_CONNECT_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          connectSharedDrivePage: {
            ...action.payload,
          },
        },
      };
    }
    case "SET_EGRESS_CONNECT_CONFIRMATION_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          egressConnectConfirmationPage: {
            ...action.payload,
          },
        },
      };
    }
    case "SET_SHARED_DRIVE_CONNECT_CONFIRMATION_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          connectSharedDriveConfirmationPage: {
            ...action.payload,
          },
        },
      };
    }
    case "SET_EGRESS_CONNECT_FAILURE_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          connectEgressFailurePage: {
            ...action.payload,
          },
        },
      };
    }
    case "SET_SHARED_DRIVE_CONNECT_FAILURE_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          connectSharedDriveFailurePage: {
            ...action.payload,
          },
        },
      };
    }

    case "SET_TRANSFER_DESTINATION_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          transferDestinationPage: {
            ...action.payload,
          },
        },
      };
    }
    case "SET_TRANSFER_RESOLVE_FILE_PATH_PAGE": {
      return {
        ...state,
        appData: {
          ...state.appData,
          transferResolveFilePathPage: {
            ...action.payload,
          },
        },
      };
    }

    default:
      return state;
  }
};
