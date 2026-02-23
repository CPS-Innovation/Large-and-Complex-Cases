export const sortByStringProperty = <T>(
  data: T[],
  stringProperty: keyof T,
  order: "ascending" | "descending",
): T[] => {
  return [...data].sort((a, b) => {
    const valA = String(a[stringProperty]).toLowerCase();
    const valB = String(b[stringProperty]).toLowerCase();

    return order === "ascending"
      ? valA.localeCompare(valB)
      : valB.localeCompare(valA);
  });
};

export const sortByDateProperty = <T>(
  data: T[],
  dateProperty: keyof T, //expected in  ISO 8601 format (YYYY-MM-DD)
  order: "ascending" | "descending",
): T[] => {
  return [...data].sort((a, b) => {
    const dateA = new Date(a[dateProperty] as string).getTime();
    const dateB = new Date(b[dateProperty] as string).getTime();

    if (order === "ascending") {
      return dateA - dateB;
    } else {
      return dateB - dateA;
    }
  });
};

export const sortByNumberProperty = <T>(
  data: T[],
  filesizeProperty: keyof T,
  order: "ascending" | "descending",
): T[] => {
  return [...data].sort((a, b) => {
    const sizeA = a[filesizeProperty] as number;
    const sizeB = b[filesizeProperty] as number;
    if (order === "ascending") return sizeA - sizeB;
    return sizeB - sizeA;
  });
};
