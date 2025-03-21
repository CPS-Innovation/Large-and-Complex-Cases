import { useEffect } from "react";
// import { useApi } from "./useApi";
import { getCaseDivisionsOrAreas } from "../../apis/gateway-api";
import { DispatchType } from "../../reducers/mainStateReducer";
import { AsyncResult } from "../types/AsyncResult";
import { CaseDivisionsOrAreaResponse } from "../types/LooksupData";

export const useGetCaseDivisionOrAreas = (
  caseDivisionsOrAreas: AsyncResult<CaseDivisionsOrAreaResponse>,
  dispatch: DispatchType,
) => {
  useEffect(() => {
    const fetchData = async () => {
      dispatch({
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "loading" },
      });
      const result = await getCaseDivisionsOrAreas();
      dispatch({
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "succeeded", data: result },
      });
    };
    if (caseDivisionsOrAreas.status !== "succeeded") fetchData();
  }, []);
};
