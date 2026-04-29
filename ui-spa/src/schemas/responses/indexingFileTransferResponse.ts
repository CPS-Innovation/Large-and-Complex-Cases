import { z } from "zod";

export const indexingErrorSchema = z.object({
  id: z.string(),
  sourcePath: z.string(),
});

export const indexingFileTransferResponseSchema = z.object({
  caseId: z.number(),
  isInvalid: z.boolean(),
  destinationPath: z.string(),
  validationErrors: z.array(indexingErrorSchema),
  sourceRootFolderPath: z.string(),
  transferDirection: z.enum(["EgressToNetApp", "NetAppToEgress"]),
  files: z.array(
    z.object({
      id: z.string().optional(),
      sourcePath: z.string(),
      relativePath: z.string().optional(),
      fullFilePath: z.string().optional(),
    }),
  ),
});

export type IndexingError = z.infer<typeof indexingErrorSchema>;
export type IndexingFileTransferResponse = z.infer<
  typeof indexingFileTransferResponseSchema
>;
