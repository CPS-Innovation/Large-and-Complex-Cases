import { z } from "zod";

export const egressFolderSchema = z.object({
  id: z.string(),
  name: z.string(),
  isFolder: z.boolean(),
  dateUpdated: z.string(),
  filesize: z.number(),
  path: z.string(),
});

export const egressFolderDataSchema = z.array(egressFolderSchema);

export const egressFolderResponseSchema = z.object({
  data: egressFolderDataSchema,
  pagination: z.object({
    totalResults: z.number(),
    skip: z.number(),
    take: z.number(),
    count: z.number(),
  }),
});

export type EgressFolder = z.infer<typeof egressFolderSchema>;
export type EgressFolderData = z.infer<typeof egressFolderDataSchema>;
export type EgressFolderResponse = z.infer<typeof egressFolderResponseSchema>;
