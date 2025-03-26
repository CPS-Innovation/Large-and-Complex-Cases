import { useMemo, useEffect } from "react";
import { useMainStateContext } from "../../providers/MainStateProvider";
import { useAsyncActionHandlers } from "../hooks/useAsyncActionHandlers";
import { CaseDivisionsOrArea } from "../types/LooksupData";

const mapGroupHeader = (text: string) => ({
  value: "",
  children: text,
  disabled: true,
});

const mapOption = (item: CaseDivisionsOrArea) => ({
  value: item.id,
  children: item.description,
});

export const useFormattedAreaValues = (makeCall: boolean = true) => {
  const {
    state: { caseDivisionsOrAreas },
  } = useMainStateContext()!;
  const { handleGetCaseDivisionsOrAreas } = useAsyncActionHandlers();

  useEffect(() => {
    if (caseDivisionsOrAreas.status !== "succeeded" && makeCall)
      handleGetCaseDivisionsOrAreas();
  }, []);
  const formattedAreaValues = useMemo(() => {
    if (caseDivisionsOrAreas.status === "failed")
      throw new Error(`${caseDivisionsOrAreas.error}`);

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

  return formattedAreaValues;
};
