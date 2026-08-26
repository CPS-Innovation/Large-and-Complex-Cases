import { useEffect, useContext } from "react";
import { useNavigate } from "react-router";
import { MainStateContext } from "../../providers/MainStateProvider";
import { getCaseDivisionsOrAreas } from "../../apis/gateway-api";
import { ApiError } from "../../common/errors/ApiError";
import { useQuery } from "@tanstack/react-query";
export const useGetCaseDivisionsOrAreas = () => {
  const navigate = useNavigate();
  const { state, dispatch } = useContext(MainStateContext);
  const { apiData: { caseDivisionsOrAreas } = {} } = state;
  const {
    data: divisionsOrAreas,
    isLoading,
    error,
    isError,
  } = useQuery({
    queryKey: [`caseDivisionsOrAreas`],
    queryFn: () => getCaseDivisionsOrAreas(),
    retry: false,
    enabled: !caseDivisionsOrAreas,
  });

  useEffect(() => {
    if (isError && error instanceof ApiError) {
      if (error.code === 401) {
        navigate("/unauthorised", { replace: true });
        return;
      }
      throw error;
    }
  }, [isError, error, navigate]);

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
