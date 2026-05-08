import { z } from "zod";

export const egressTransferPayloadSourcePathSchema = z.object({
  fileId: z.string().optional(),
  path: z.string(),
  modifiedPath: z.string().optional(),
  fullFilePath: z.string().optional(),
});
export const egreessToNetAppTransferPayloadSchema = z.object({
  workspaceId: z.string(),
  caseId: z.number(),
  transferType: z.enum(["Copy", "Move"]),
  transferDirection: z.literal("EgressToNetApp"),
  sourcePaths: z.array(egressTransferPayloadSourcePathSchema),
  sourceRootFolderPath: z.string(),
  destinationPath: z.string(),
});
export const netAppTransferPayloadSourcePathSchema = z.object({
  path: z.string(),
  relativePath: z.string().optional(),
});
export const netAppToEgressTransferPayloadSchema = z.object({
  workspaceId: z.string(),
  caseId: z.number(),
  transferType: z.literal("Copy"),
  transferDirection: z.literal("NetAppToEgress"),
  sourcePaths: z.array(netAppTransferPayloadSourcePathSchema),
  sourceRootFolderPath: z.string(),
  destinationPath: z.string(),
});
export const initiateFileTransferPayloadSchema = z.union([
  netAppToEgressTransferPayloadSchema,
  egreessToNetAppTransferPayloadSchema,
]);

export type EgreessToNetAppTransferPayload = z.infer<
  typeof egreessToNetAppTransferPayloadSchema
>;
export type NetAppToEgressTransferPayload = z.infer<
  typeof netAppToEgressTransferPayloadSchema
>;
export type NetAppTransferPayloadSourcePath = z.infer<
  typeof netAppTransferPayloadSourcePathSchema
>;
export type EgressTransferPayloadSourcePath = z.infer<
  typeof egressTransferPayloadSourcePathSchema
>;
export type InitiateFileTransferPayload = z.infer<
  typeof initiateFileTransferPayloadSchema
>;
