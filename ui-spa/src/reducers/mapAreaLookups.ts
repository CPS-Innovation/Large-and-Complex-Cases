import {
  CaseDivisionsOrArea,
  CaseDivisionsOrAreaResponse,
} from "../common/types/LooksupData";

const areaSortFn = (a: CaseDivisionsOrArea, b: CaseDivisionsOrArea) =>
  a.description < b.description ? -1 : a.description > b.description ? 1 : 0;

export const mapAreaLookups = ({
  allAreas,
  homeArea,
  userAreas,
}: CaseDivisionsOrAreaResponse): CaseDivisionsOrAreaResponse => ({
  userAreas: userAreas.sort(areaSortFn),
  allAreas: allAreas.sort(areaSortFn),
  homeArea,
});
