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
  // Load lookups
  // const caseDivisionsOrAreasData = useApi(getCaseDivisionsOrAreas, [], true);
  console.log("getCaseDivisionsOrAreas>>>", caseDivisionsOrAreas);
  // useEffect(() => {
  //   if (caseDivisionsOrAreasData.status !== "initial") {
  //     console.log("hiiii>----");
  //     // dispatch({
  //     //   type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
  //     //   payload: caseDivisionsOrAreasData,
  //     // });
  //   }
  // }, [caseDivisionsOrAreasData, dispatch]);

  useEffect(() => {
    const fetchData = async () => {
      dispatch({
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "loading" },
      });
      const result = await getCaseDivisionsOrAreas();
      console.log("result>>", result);
      dispatch({
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "succeeded", data: result },
      });
    };
    if (caseDivisionsOrAreas.status !== "succeeded") fetchData();
  }, []);
};
