import { EgressFolderResponse } from "../../common/types/EgressFolderData";
export const egressRootFolderResultsPlaywright: EgressFolderResponse = {
  data: [
    {
      id: "1-0",
      name: "folder-1-0",
      isFolder: true,
      dateUpdated: "2000-01-02",
      filesize: 0,
      path: "",
    },
    {
      id: "1-1",
      name: "folder-1-1",
      isFolder: true,
      dateUpdated: "2000-01-03",
      filesize: 0,
      path: "",
    },
    {
      id: "1-2",
      name: "file-1-2.pdf",
      isFolder: false,
      dateUpdated: "2000-01-03",
      filesize: 1234,
      path: "",
    },
  ],

  pagination: {
    totalResults: 50,
    skip: 0,
    take: 50,
    count: 25,
  },
};

const getFolderPathFromId = (id: number, index: number) => {
  let path = "";
  for (let i = 1; i < id; i++) {
    if (path === "") path = `folder-${i}-${index}`;
    else path = `${path}/folder-${i}-${index}`;
  }
  return path;
};
export const getEgressFolderResultsPlaywright = (
  id: string,
): EgressFolderResponse => {
  if (!id) return egressRootFolderResultsPlaywright;

  const newId = parseInt(id.split("-")[0]) + 1;
  if (newId > 3) {
    return {
      ...egressRootFolderResultsPlaywright,
      data: [],
    };
  }
  const newFolders = egressRootFolderResultsPlaywright.data.map(
    (item, index) => {
      return {
        ...item,
        id: `${newId}-${index}`,
        name: `folder-${newId}-${index}`,
        path: getFolderPathFromId(newId, index),
      };
    },
  );

  return {
    ...egressRootFolderResultsPlaywright,
    data: newFolders,
  };
};
