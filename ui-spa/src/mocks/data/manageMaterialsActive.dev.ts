import { type ManageMaterialsActiveResponse } from "../../schemas";

// Paths overlap the dev NetApp folder mock data (netappFolderResults.dev.ts),
// so the rows for folder-1-0 and folder-1-2 (and everything inside them)
// render with disabled checkboxes on the Manage Materials tab.
export const manageMaterialsLockedPathsDev = [
  "netapp/folder-1-0/",
  "netapp/folder-1-2/",
];

export const manageMaterialsActiveDev: ManageMaterialsActiveResponse = [
  {
    id: "active-op-1",
    operationType: "BatchCopy",
    sourcePaths: JSON.stringify([manageMaterialsLockedPathsDev[0]]),
    destinationPaths: JSON.stringify([manageMaterialsLockedPathsDev[1]]),
    userName: "another.user@example.org",
    createdAt: "2026-06-10T08:00:00Z",
  },
];
