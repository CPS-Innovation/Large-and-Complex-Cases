import { z } from "zod";

export const connectNetAppFolderSchema = z.object({
  folderPath: z.string(),
  caseId: z.number().nullable(),
});
export const connectNetAppFolderDataSchema = z.object({
  rootPath: z.string(),
  folders: z.array(connectNetAppFolderSchema),
  // Present when the user cannot list the bucket root and the folders above are the entry
  // prefixes they were found to have access to. Only returned for the root listing.
  isRestrictedRoot: z.boolean().optional(),
  accessibleRoots: z.array(z.string()).nullish(),
});
export const connectNetAppFolderResponseSchema = z.object({
  data: connectNetAppFolderDataSchema,
  pagination: z.object({
    maxKeys: z.number(),
    nextContinuationToken: z.string().nullable(),
  }),
});

export type ConnectNetAppFolder = z.infer<typeof connectNetAppFolderSchema>;
export type ConnectNetAppFolderData = z.infer<
  typeof connectNetAppFolderDataSchema
>;
export type ConnectNetAppFolderResponse = z.infer<
  typeof connectNetAppFolderResponseSchema
>;
