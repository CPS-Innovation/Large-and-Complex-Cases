import { useMainStateContext } from "../../providers/MainStateProvider";

export const useAsyncActionHandlers = () => {
  const { dispatch } = useMainStateContext()!;
  const handleGetCaseDivisionsOrAreas = () => {
    dispatch({
      type: "GET_CASE_DIVISIONS_OR_AREAS",
    });
  };

  return {
    handleGetCaseDivisionsOrAreas,
  };
};
