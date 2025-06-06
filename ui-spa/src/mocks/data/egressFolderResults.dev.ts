import { EgressFolderResponse } from "../../common/types/EgressFolderData";
export const egressRootFolderResultsDev: EgressFolderResponse = {
  data: [
    {
      id: "1-0",
      name: "folder-1-0",
      isFolder: true,
      dateUpdated: "2000-01-02",
      filesize: 0,
      path: "egress/",
    },
    {
      id: "1-1",
      name: "folder-1-1",
      isFolder: true,
      dateUpdated: "2000-01-03",
      filesize: 0,
      path: "egress/",
    },
    {
      id: "1-2",
      name: "file-1-2.pdf",
      isFolder: false,
      dateUpdated: "2000-01-03",
      filesize: 1234,
      path: "egress/",
    },
  ],

  pagination: {
    totalResults: 50,
    skip: 0,
    take: 50,
    count: 25,
  },
};

const getFolderPathFromId = (id: number, index: number, rootPath: string) => {
  let path = "";
  for (let i = 1; i < id; i++) {
    if (path === "") path = `${rootPath}folder-${i}-${index}/`;
    else path = `${path}folder-${i}-${index}/`;
  }
  return path;
};
export const getEgressFolderResultsDev = (id: string): EgressFolderResponse => {
  if (id === "egress_1") return egressRootFolderResultsDev;
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
      path: getFolderPathFromId(newId, index, item.path),
    };
  });

  return {
    ...egressRootFolderResultsDev,
    data: newFolders,
  };
};
