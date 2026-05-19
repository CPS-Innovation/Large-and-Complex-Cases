export const getUrlSearchParam = (
  property: string,
  value: string | null | undefined,
) => {
  return new URLSearchParams({ [property]: value ?? "`" });
};
