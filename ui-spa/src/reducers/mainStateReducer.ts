import { type CaseDivisionsOrAreaResponse } from "../schemas";
import { mapAreaLookups } from "./utils/mapAreaLookups";

export type MainState = {
  apiData: {
    caseDivisionsOrAreas: CaseDivisionsOrAreaResponse | null;
  };
};

export const initialState: MainState = {
  apiData: {
    caseDivisionsOrAreas: null,
  },
};

export type MainStateActions = {
  type: "SET_CASE_DIVISIONS_OR_AREAS";
  payload: {
    caseDivisionsOrAreas: CaseDivisionsOrAreaResponse;
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

    default:
      return state;
  }
};
