import { useMemo } from "react";
import { useMainStateContext } from "../../providers/mainStateProvider";
import { useGetCaseDivisionOrAreas } from "../../common/hooks/useGetAppLevelLookups";
import { CaseDivisionsOrArea } from "../types/LooksupData";

const mapGroupHeader = (text: string) => ({
  value: -1,
  children: text,
  disabled: true,
});

const mapOption = (item: CaseDivisionsOrArea) => ({
  value: item.id,
  children: item.description,
});

export const useFormattedAreaValues = () => {
  const {
    state: { caseDivisionsOrAreas },
    dispatch,
  } = useMainStateContext()!;

  useGetCaseDivisionOrAreas(caseDivisionsOrAreas, dispatch);

  const formattedAreaValues = useMemo(() => {
    if (caseDivisionsOrAreas.status !== "succeeded")
      return { defaultValue: undefined, options: [] };

    return {
      defaultValue: caseDivisionsOrAreas.data.homeArea.id,
      options: [
        mapGroupHeader("-- Please select --"),
        mapGroupHeader("Your units/areas"),
        ...caseDivisionsOrAreas.data.userAreas.map(mapOption),
        mapGroupHeader("All areas"),
        ...caseDivisionsOrAreas.data.allAreas.map(mapOption),
      ],
    };
  }, [caseDivisionsOrAreas]);

  console.log(formattedAreaValues.defaultValue);

  return formattedAreaValues;
};
