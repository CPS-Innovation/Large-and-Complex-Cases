export const getRelativePathFromPath = (path: string) => {
  const regex = /(.*)\/[^/]+$/;
  const match = regex.exec(path);
  return match ? match[1] : "";
};
