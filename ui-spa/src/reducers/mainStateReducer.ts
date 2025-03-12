import { CaseDivisionsOrArea } from "../common/types/LooksupData";
import { ApiResult } from "../common/types/ApiResult";
import { AsyncResult } from "../common/types/AsyncResult";

export type MainState = {
  caseDivisionsOrAreas: AsyncResult<CaseDivisionsOrArea[]>;
};

export const initialState: MainState = {
  caseDivisionsOrAreas: { status: "loading" },
};

export type MainStateActions = {
  type: "UPDATE_CASE_DIVISIONS_OR_AREAS";
  payload: ApiResult<CaseDivisionsOrArea[]>;
};

export type DispatchType = React.Dispatch<MainStateActions>;

export const mainStateReducer = (
  state: MainState,
  action: MainStateActions,
): MainState => {
  switch (action.type) {
    case "UPDATE_CASE_DIVISIONS_OR_AREAS": {
      console.log("hiiii", action.payload);
      if (action.payload.status === "failed") return state;
      return {
        ...state,
        caseDivisionsOrAreas: action.payload,
      };
    }

    default:
      return state;
  }
};
