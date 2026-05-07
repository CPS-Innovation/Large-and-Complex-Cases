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
      id: z.string().nullable(),
      sourcePath: z.string(),
      relativePath: z.string().nullable(),
      fullFilePath: z.string().nullable(),
    }),
  ),
});

export type IndexingError = z.infer<typeof indexingErrorSchema>;
export type IndexingFileTransferResponse = z.infer<
  typeof indexingFileTransferResponseSchema
>;
