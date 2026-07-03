import { type CaseDivisionsOrAreaResponse } from "../schemas";
import { mapAreaLookups } from "./utils/mapAreaLookups";
import { FeatureFlagData } from "../common/types/FeatureFlagData";

export type MainState = {
  appData: {
    featureFlags: FeatureFlagData | null;
  };
  apiData: {
    caseDivisionsOrAreas: CaseDivisionsOrAreaResponse | null;
  };
};

export const initialState: MainState = {
  appData: {
    featureFlags: null,
  },
  apiData: {
    caseDivisionsOrAreas: null,
  },
};

export type MainStateActions =
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

    default:
      return state;
  }
};
