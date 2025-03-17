import { useMemo } from "react";
import { useMainStateContext } from "../../providers/MainStateProvider";
import { useGetCaseDivisionOrAreas } from "../../common/hooks/useGetAppLevelLookups";

export const useFormattedAreaValues = () => {
  const {
    state: { caseDivisionsOrAreas },
    dispatch,
  } = useMainStateContext()!;
  useGetCaseDivisionOrAreas(caseDivisionsOrAreas, dispatch);

  const formattedAreaValues = useMemo(() => {
    if (caseDivisionsOrAreas.status !== "succeeded") return [];
    const defaultOption = {
      value: "",
      children: "-- Please select --",
      disabled: true,
    };
    const optionGroup1 = caseDivisionsOrAreas.data
      .filter((item: any) => item.type === "Large and Complex Case Divisions")
      .map((item: any) => ({
        value: item.code,
        children: item.name,
        disabled: false,
      }));
    const optionGroup2 = caseDivisionsOrAreas.data
      .filter((item: any) => item.type === "CPS Areas")
      .map((item: any) => ({
        value: item.code,
        children: item.name,
        disabled: false,
      }));
    return [defaultOption, ...optionGroup1, ...optionGroup2];
  }, [caseDivisionsOrAreas]);

  return formattedAreaValues;
};
