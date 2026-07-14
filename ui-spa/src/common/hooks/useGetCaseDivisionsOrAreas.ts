import { useEffect, useContext } from "react";
import { MainStateContext } from "../../providers/MainStateProvider";
import { getCaseDivisionsOrAreas } from "../../apis/gateway-api";
import { useQuery } from "@tanstack/react-query";
export const useGetCaseDivisionsOrAreas = () => {
  const { state, dispatch } = useContext(MainStateContext);
  const { apiData: { caseDivisionsOrAreas } = {} } = state;
  const { data: divisionsOrAreas, isLoading } = useQuery({
    queryKey: [`caseDivisionsOrAreas`],
    queryFn: () => getCaseDivisionsOrAreas(),
    retry: false,
    enabled: !caseDivisionsOrAreas,
    throwOnError: true,
  });
  useEffect(() => {
    if (divisionsOrAreas && !caseDivisionsOrAreas) {
      dispatch({
        type: "SET_CASE_DIVISIONS_OR_AREAS",
        payload: {
          caseDivisionsOrAreas: divisionsOrAreas,
        },
      });
    }
  }, [divisionsOrAreas, caseDivisionsOrAreas, dispatch]);
  return {
    isLoading,
  };
};
