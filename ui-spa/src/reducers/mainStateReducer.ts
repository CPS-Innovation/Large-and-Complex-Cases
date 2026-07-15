import { type CaseDivisionsOrAreaResponse } from "../schemas";
import { mapAreaLookups } from "./utils/mapAreaLookups";
import { FeatureFlagData } from "../common/types/FeatureFlagData";

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
  };
  formData: FormData;
  apiData: {
    caseDivisionsOrAreas: CaseDivisionsOrAreaResponse | null;
  };
};

export const initialState: MainState = {
  appData: {
    featureFlags: null,
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
      type: "SET_FEATURE_FLAGS";
      payload: {
        featureFlags: FeatureFlagData;
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

    default:
      return state;
  }
};
