export type CaseDivisionsOrArea = {
  id: number;
  description: string;
  default?: true;
};

export type CaseDivisionsOrAreaResponse = {
  allAreas: CaseDivisionsOrArea[];
  userAreas: CaseDivisionsOrArea[];
  homeArea: CaseDivisionsOrArea;
};
