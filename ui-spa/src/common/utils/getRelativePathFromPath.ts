export const getRelativePathFromPath = (path: string) => {
  const trimmedPath = path.replace(/\/$/, "");
  const regex = /(.*)\/[^/]+$/;
  const match = regex.exec(trimmedPath);
  return match ? match[1] : "";
};
