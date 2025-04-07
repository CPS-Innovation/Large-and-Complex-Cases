export const sortByStringProperty = <T>(
  data: T[],
  property: keyof T,
  order: "ascending" | "descending",
): T[] => {
  return [...data].sort((a, b) => {
    const valA = String(a[property]).toLowerCase();
    const valB = String(b[property]).toLowerCase();

    return order === "ascending"
      ? valA.localeCompare(valB)
      : valB.localeCompare(valA);
  });
};

export const sortByDateProperty = <T>(
  data: T[],
  property: keyof T,
  order: "ascending" | "descending",
): T[] => {
  return [...data].sort((a, b) => {
    const dateA = new Date(a[property] as string).getTime();
    const dateB = new Date(b[property] as string).getTime();

    if (order === "ascending") {
      return dateA - dateB;
    } else {
      return dateB - dateA;
    }
  });
};
