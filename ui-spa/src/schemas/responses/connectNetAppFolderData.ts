import { z } from "zod";

export const connectNetAppFolderSchema = z.object({
  folderPath: z.string(),
  caseId: z.number().nullable(),
});
export const connectNetAppFolderDataSchema = z.object({
  rootPath: z.string(),
  folders: z.array(connectNetAppFolderSchema),
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
