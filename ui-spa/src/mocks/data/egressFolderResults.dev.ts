import { EgressFolderResponse } from "../../common/types/EgressFolderData";
export const egressRootFolderResultsDev: EgressFolderResponse = {
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
      name: "folder-1-2",
      isFolder: true,
      dateUpdated: "2000-01-02",
      filesize: 0,
      path: "",
    },
    {
      id: "1-3",
      name: "folder-1-3",
      isFolder: true,
      dateUpdated: "2000-01-03",
      filesize: 0,
      path: "",
    },
    {
      id: "1-4",
      name: "folder-1-4",
      isFolder: true,
      dateUpdated: "2000-01-02",
      filesize: 0,
      path: "",
    },
    {
      id: "1-5",
      name: "folder-1-5",
      isFolder: true,
      dateUpdated: "2000-01-03",
      filesize: 0,
      path: "",
    },
    {
      id: "1-6",
      name: "folder-1-6",
      isFolder: true,
      dateUpdated: "2000-01-02",
      filesize: 0,
      path: "",
    },
    {
      id: "1-7",
      name: "folder-1-7",
      isFolder: true,
      dateUpdated: "2000-01-03",
      filesize: 0,
      path: "",
    },

    {
      id: "1-8",
      name: "file3qeeweweweweewweewwewewewewewewweewerwrrwwrwrrrrrrwrwrwrwweewweeewweweeweweweew.pdf",
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

const getFolderPathFromId = (id: string) => {
  const ids = id.split(",");
  let path = "";
  ids.forEach((id) => {
    if (path) path += "/";
    path += `folder-${id}`;
  });

  return path;
};

export function getLastSegment(input: string): string {
  if (!input) return "";
  const parts = input
    .split(",")
    .map((p) => p.trim())
    .filter(Boolean);
  return parts.length ? parts[parts.length - 1] : "";
}
export const getEgressFolderResultsDev = (id: string): EgressFolderResponse => {
  if (!id) return egressRootFolderResultsDev;
  const lastSegment = getLastSegment(id);
  const newId = parseInt(lastSegment.split("-")[0]) + 1;
  if (newId > 3) {
    return {
      ...egressRootFolderResultsDev,
      data: [],
    };
  }
  const newFolders = egressRootFolderResultsDev.data.map((item, index) => {
    return {
      ...item,
      id: `${id},${newId}-${index}`,
      name: index === 8 ? `files-${newId}-0.pdf` : `folder-${newId}-${index}`,
      path: `${getFolderPathFromId(id)}`,
    };
  });

  return {
    ...egressRootFolderResultsDev,
    data: newFolders,
  };
};
