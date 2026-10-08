import { type ConnectNetAppFolderResponse } from "../../schemas";
export const netAppRootFolderResultsDev: ConnectNetAppFolderResponse = {
  data: {
    rootPath: "",
    folders: [
      {
        folderPath: "thunderstrikeab/",
        caseId: 123,
      },
      {
        folderPath: "thunderstrike/",
        caseId: null,
      },

      {
        folderPath: "thunderstrikeabc/",
        caseId: null,
      },
    ],
  },

  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

// Mirrors the API response for a user whose NTFS permissions are scoped to subfolders: the
// bucket root is not listable, so the accessible entry prefixes are returned instead.
export const netAppRestrictedRootFolderResultsDev: ConnectNetAppFolderResponse =
  {
    data: {
      rootPath: "",
      folders: [
        {
          folderPath: "RCF/",
          caseId: null,
        },
        {
          folderPath: "RCW/Cardiff/",
          caseId: null,
        },
      ],
      isRestrictedRoot: true,
      accessibleRoots: ["RCF/", "RCW/Cardiff/"],
    },

    pagination: {
      maxKeys: 2,
      nextContinuationToken: null,
    },
  };

// Browse with ?operation-name=restricted to exercise the prefix-scoped flow locally.
export const RESTRICTED_ROOT_OPERATION_NAME = "restricted";

export const getConnectNetAppFolderResultsDev = (
  path: string,
  operationName?: string | null,
) => {
  // Only the root listing reports the restriction, matching the API, which falls back to the entry
  // prefixes just for the bucket root.
  const rootResults =
    operationName === RESTRICTED_ROOT_OPERATION_NAME
      ? netAppRestrictedRootFolderResultsDev
      : netAppRootFolderResultsDev;

  if (!path || path === "abc") return rootResults;

  const levels = path.split("/").filter((part) => part.length > 0);
  if (levels.length > 3) {
    return {
      ...rootResults,
      data: {
        rootPath: path,
        folders: [],
      },
    };
  }
  const newFolders = rootResults.data.folders.map((item, index) => {
    return { ...item, folderPath: `${path}folder-${index}/` };
  });

  return {
    ...rootResults,
    data: {
      rootPath: path,
      folders: newFolders,
    },
  };
};
