export const getFolderNameFromPath = (path: string) => {
  console.log(path);
  const match = path.match(/([^/]+)\/$/);
  return match ? match[1] : "";
};
