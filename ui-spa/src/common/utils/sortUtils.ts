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
  dateProperty: keyof T,
  order: "ascending" | "descending",
): T[] => {
  return [...data].sort((a, b) => {
    let dateA = new Date(a[dateProperty] as string).getTime();
    let dateB = new Date(b[dateProperty] as string).getTime();

    if (Number.isNaN(dateA))
      dateA =
        order === "ascending"
          ? Number.POSITIVE_INFINITY
          : Number.NEGATIVE_INFINITY;
    if (Number.isNaN(dateB))
      dateB =
        order === "ascending"
          ? Number.POSITIVE_INFINITY
          : Number.NEGATIVE_INFINITY;

    if (order === "ascending") return dateA - dateB;

    return dateB - dateA;
  });
};

export const sortByNumberProperty = <T>(
  data: T[],
  numberProperty: keyof T,
  order: "ascending" | "descending",
): T[] => {
  return [...data].sort((a, b) => {
    let sizeA = a[numberProperty] as number;
    let sizeB = b[numberProperty] as number;

    if (!sizeA)
      sizeA =
        order === "ascending"
          ? Number.POSITIVE_INFINITY
          : Number.NEGATIVE_INFINITY;
    if (!sizeB)
      sizeB =
        order === "ascending"
          ? Number.POSITIVE_INFINITY
          : Number.NEGATIVE_INFINITY;

    if (order === "ascending") return sizeA - sizeB;
    return sizeB - sizeA;
  });
};
