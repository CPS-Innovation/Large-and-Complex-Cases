import { type EgressFolderResponse } from "../../schemas";
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
export const getEgressFolderResultsDev = (
  id: string,
  path: string,
): EgressFolderResponse => {
  if (!id && !path) return egressRootFolderResultsDev;
  if (!id) {
    id = path
      .split("/")
      .map((part) => part.replace("folder-", ""))
      .join(",");
  }
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
      name:
        index === 2 ? `file-${newId}-${index}.pdf` : `folder-${newId}-${index}`,
      path: `${getFolderPathFromId(id)}`,
    };
  });

  return {
    ...egressRootFolderResultsDev,
    data: newFolders,
  };
};
