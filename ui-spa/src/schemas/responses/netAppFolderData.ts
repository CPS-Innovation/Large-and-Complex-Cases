import { z } from "zod";
export const netAppFolderSchema = z.object({
  path: z.string(),
});
export const netAppFileSchema = z.object({
  path: z.string(),
  lastModified: z.string(),
  filesize: z.number(),
});

export const NetAppFolderDataResponseSchema = z.object({
  fileData: z.array(netAppFileSchema),
  folderData: z.array(netAppFolderSchema),
});

export const netAppFolderResponseSchema = z.object({
  data: NetAppFolderDataResponseSchema,
  pagination: z.object({
    maxKeys: z.number(),
    nextContinuationToken: z.string().nullable(),
  }),
});

export const netAppFolderDataSchema = z.array(
  z.object({
    path: z.string(),
    lastModified: z.string(),
    filesize: z.number(),
    isFolder: z.boolean(),
  }),
);

export type NetAppFolder = z.infer<typeof netAppFolderSchema>;
export type NetAppFile = z.infer<typeof netAppFileSchema>;
export type NetAppFolderDataResponse = z.infer<
  typeof NetAppFolderDataResponseSchema
>;
export type NetAppFolderResponse = z.infer<typeof netAppFolderResponseSchema>;
export type NetAppFolderData = z.infer<typeof netAppFolderDataSchema>;
