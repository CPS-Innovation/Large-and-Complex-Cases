import { useMemo } from "react";
import { useMainStateContext } from "../../providers/mainStateProvider";
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
      .filter(
        (item) =>
          item.type === "Large and Complex Case Divisions" || !item.type,
      )
      .map((item) => ({
        value: item.code,
        children: item.name,
        disabled: false,
      }));
    const optionGroup2 = caseDivisionsOrAreas.data
      .filter((item) => item.type === "CPS Areas")
      .map((item) => ({
        value: item.code,
        children: item.name,
        disabled: false,
      }));
    return [
      defaultOption,
      ...optionGroup1,
      ...optionGroup2,
      { value: "1001", children: "Surrey", type: "CPS Areas" },
    ];
  }, [caseDivisionsOrAreas]);

  return formattedAreaValues;
};
