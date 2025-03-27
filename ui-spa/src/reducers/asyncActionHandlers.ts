import { AsyncActionHandlers } from "use-reducer-async";
import { MainState, MainStateActions } from "./mainStateReducer";
import { Reducer } from "react";
import { getCaseDivisionsOrAreas } from "../apis/gateway-api";

export interface AsyncActions {
  type: "GET_CASE_DIVISIONS_OR_AREAS";
}

export const asyncActionHandlers: AsyncActionHandlers<
  Reducer<MainState, MainStateActions>,
  AsyncActions
> = {
  GET_CASE_DIVISIONS_OR_AREAS:
    ({ dispatch }) =>
    async (_action) => {
      dispatch({
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "loading" },
      });
      try {
        const result = await getCaseDivisionsOrAreas();
        dispatch({
          type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
          payload: { status: "succeeded", data: result },
        });
      } catch (error) {
        dispatch({
          type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
          payload: { status: "failed", error: `${error}` },
        });
      }
    },
};
