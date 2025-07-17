export const sortRelativePaths = (paths: string[]) => {
  const result = paths.toSorted((a, b) => {
    const countA = a ? (a.match(/>/g) || []).length : -1;
    const countB = b ? (b.match(/>/g) || []).length : -1;
    return countA - countB;
  });
  console.log("result>>>", result);
  return result;
};
