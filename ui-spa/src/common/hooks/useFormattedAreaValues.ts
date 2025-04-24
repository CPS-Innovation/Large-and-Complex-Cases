import { useMemo, useEffect } from "react";
import { useMainStateContext } from "../../providers/MainStateProvider";
import { useAsyncActionHandlers } from "../hooks/useAsyncActionHandlers";
import { CaseDivisionsOrArea } from "../types/LooksupData";
import { useLocation } from "react-router";

const mapGroupHeader = (text: string) => ({
  value: "",
  children: text,
  disabled: true,
});

const mapOption = (item: CaseDivisionsOrArea) => ({
  value: item.id,
  children: item.description,
});

export const useFormattedAreaValues = (isUrnSearch: boolean = false) => {
  const {
    state: { caseDivisionsOrAreas },
  } = useMainStateContext()!;
  const { handleGetCaseDivisionsOrAreas } = useAsyncActionHandlers();

  const { pathname } = useLocation();

  useEffect(() => {
    if (caseDivisionsOrAreas.status !== "succeeded" && !isUrnSearch)
      handleGetCaseDivisionsOrAreas();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
  const formattedAreaValues = useMemo(() => {
    if (
      caseDivisionsOrAreas.status === "failed" &&
      pathname === "/search-results" &&
      !isUrnSearch
    )
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
  }, [caseDivisionsOrAreas, isUrnSearch, pathname]);

  return formattedAreaValues;
};
