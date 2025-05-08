import { EgressFolderResponse } from "../../common/types/EgressFolderData";
export const egressRootFolderResultsDev: EgressFolderResponse = {
  data: [
    {
      id: "1-0",
      name: "folder-1-0",
      isFolder: true,
      dateUpdated: "2000-01-02",
      path: "",
    },
    {
      id: "1-1",
      name: "folder-1-1",
      isFolder: true,
      dateUpdated: "2000-01-03",
      path: "",
    },
    {
      id: "1-2",
      name: "file-1-2.pdf",
      isFolder: false,
      dateUpdated: "2000-01-03",
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
export const getEgressFolderResultsDev = (id: string): EgressFolderResponse => {
  console.log("id>>>>>>", id);
  if (!id) return egressRootFolderResultsDev;

  const newId = parseInt(id.split("-")[0]) + 1;
  if (newId > 3) {
    return {
      ...egressRootFolderResultsDev,
      data: [],
    };
  }
  const newFolders = egressRootFolderResultsDev.data.map((item, index) => {
    return {
      ...item,
      id: `${newId}-${index}`,
      name: `folder-${newId}-${index}`,
      path: getFolderPathFromId(newId, index),
    };
  });

  return {
    ...egressRootFolderResultsDev,
    data: newFolders,
  };
};
