import { z } from "zod";

export const createSharedDriveFolderPayloadSchema = z.object({
  templateFolderPath: z.string(),
  parentFolderPath: z.string(),
});

export type CreateSharedDriveFolderPayload = z.infer<
  typeof createSharedDriveFolderPayloadSchema
>;
