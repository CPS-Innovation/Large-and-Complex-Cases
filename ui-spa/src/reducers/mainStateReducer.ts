import { CaseDivisionsOrAreaResponse } from "../common/types/LooksupData";
import { ApiResult } from "../common/types/ApiResult";
import { AsyncResult } from "../common/types/AsyncResult";
import { mapAreaLookups } from "./utils/mapAreaLookups";

export type MainState = {
  caseDivisionsOrAreas: AsyncResult<CaseDivisionsOrAreaResponse>;
};

export const initialState: MainState = {
  caseDivisionsOrAreas: { status: "loading" },
};

export type MainStateActions = {
  type: "UPDATE_CASE_DIVISIONS_OR_AREAS";
  payload: ApiResult<CaseDivisionsOrAreaResponse>;
};

export type DispatchType = React.Dispatch<MainStateActions>;

export const mainStateReducer = (
  state: MainState,
  action: MainStateActions,
): MainState => {
  switch (action.type) {
    case "UPDATE_CASE_DIVISIONS_OR_AREAS": {
      switch (action.payload.status) {
        case "failed":
          return state;
        case "loading":
          return {
            ...state,
            caseDivisionsOrAreas: action.payload,
          };
        case "succeeded":
          return {
            ...state,
            caseDivisionsOrAreas: {
              ...action.payload,
              data: mapAreaLookups(action.payload.data),
            },
          };
        default:
          throw new Error(
            "Unexpected status in mainStateReducer UPDATE_CASE_DIVISIONS_OR_AREAS",
          );
      }
    }

    default:
      return state;
  }
};
