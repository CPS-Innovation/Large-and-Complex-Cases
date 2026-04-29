import { z } from "zod";

export const ConnectNetAppFolderSchema = z.object({
  folderPath: z.string(),
  caseId: z.number().nullable(),
});
export const ConnectNetAppFolderDataSchema = z.object({
  rootPath: z.string(),
  folders: z.array(ConnectNetAppFolderSchema),
});
export const ConnectNetAppFolderResponseSchema = z.object({
  data: ConnectNetAppFolderDataSchema,
  pagination: z.object({
    maxKeys: z.number(),
    nextContinuationToken: z.string().nullable(),
  }),
});

export type ConnectNetAppFolder = z.infer<typeof ConnectNetAppFolderSchema>;
export type ConnectNetAppFolderData = z.infer<
  typeof ConnectNetAppFolderDataSchema
>;
export type ConnectNetAppFolderResponse = z.infer<
  typeof ConnectNetAppFolderResponseSchema
>;
