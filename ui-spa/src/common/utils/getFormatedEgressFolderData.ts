import { EgressFolderData } from "../types/EgressFolderData";
export const getFormatedEgressFolderData = (
  data: EgressFolderData,
): EgressFolderData => {
  const mapped = data.map((item) => {
    if (item.isFolder)
      return {
        ...item,
        path: item.path ? `${item.path}/${item.name}/` : `${item.name}/`,
      };
    return {
      ...item,
      path: item.path ? `${item.path}/${item.name}` : item.name,
    };
  });
  // Sort folders first then files
  mapped.sort((a, b) => {
    if (a.isFolder === b.isFolder) return 0;
    return a.isFolder ? -1 : 1;
  });

  return mapped;
};
