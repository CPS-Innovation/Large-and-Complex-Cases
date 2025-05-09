export const getFolderNameFromPath = (path: string) => {
  const match = path.match(/([^/]+)\/$/);
  return match ? match[1] : "";
};
