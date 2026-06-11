import { useEffect } from "react";
import { useMainStateContext } from "../../providers/MainStateProvider";
import { getCaseDivisionsOrAreas } from "../../apis/gateway-api";
import { useQuery } from "@tanstack/react-query";
export const useGetCaseDivisionsOrAreas = () => {
  const {
    state: {
      apiData: { caseDivisionsOrAreas },
    },
    dispatch,
  } = useMainStateContext()!;
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
