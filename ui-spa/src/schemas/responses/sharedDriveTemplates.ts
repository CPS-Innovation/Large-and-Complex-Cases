import { z } from "zod";

export const sharedDriveTemplatesResponseSchema = z.object({
  parentFolderPath: z.string(),
  templates: z.array(
    z.object({
      name: z.string(),
      path: z.string(),
    }),
  ),
  count: z.number(),
});

export type SharedDriveTemplatesResponse = z.infer<
  typeof sharedDriveTemplatesResponseSchema
>;
