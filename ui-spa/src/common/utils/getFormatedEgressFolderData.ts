import { EgressFolderData } from "../types/EgressFolderData";
export const getFormatedEgressFolderData = (
  data: EgressFolderData,
): EgressFolderData => {
  return data.map((item) => {
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
};
