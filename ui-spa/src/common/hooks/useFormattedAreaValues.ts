import { useMemo, useContext } from "react";
import { MainStateContext } from "../../providers/MainStateProvider";
import { type CaseDivisionsOrArea } from "../../schemas";

const mapGroupHeader = (text: string) => ({
  value: "",
  children: text,
  disabled: true,
});

const mapOption = (item: CaseDivisionsOrArea) => ({
  value: item.id,
  children: item.description,
});

export const useFormattedAreaValues = () => {
  const { state } = useContext(MainStateContext) ?? {};
  const { apiData: { caseDivisionsOrAreas } = {} } = state;

  const formattedAreaValues = useMemo(() => {
    if (!caseDivisionsOrAreas) return { defaultValue: undefined, options: [] };

    return {
      defaultValue: caseDivisionsOrAreas.homeArea.id,
      options: [
        mapGroupHeader("-- Please select --"),
        mapGroupHeader("Your units/areas"),
        ...caseDivisionsOrAreas.userAreas.map(mapOption),
        mapGroupHeader("All areas"),
        ...caseDivisionsOrAreas.allAreas.map(mapOption),
      ],
    };
  }, [caseDivisionsOrAreas]);

  return formattedAreaValues;
};
