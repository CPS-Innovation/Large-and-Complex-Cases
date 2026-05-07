import { z } from "zod";

export const indexingFileTransferPayloadSchema = z.object({
  caseId: z.number(),
  transferDirection: z.enum(["EgressToNetApp", "NetAppToEgress"]),
  transferType: z.enum(["Move", "Copy"]),
  sourcePaths: z.array(
    z.object({
      fileId: z.string().optional(),
      path: z.string(),
      isFolder: z.boolean().optional(),
    }),
  ),
  destinationPath: z.string(),
  sourceRootFolderPath: z.string(),
  workspaceId: z.string().optional(),
});

export type IndexingFileTransferPayload = z.infer<
  typeof indexingFileTransferPayloadSchema
>;
