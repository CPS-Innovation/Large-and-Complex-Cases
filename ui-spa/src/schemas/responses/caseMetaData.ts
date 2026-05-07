import { z } from "zod";

export const caseMetaDataResponseSchema = z.object({
  caseId: z.number(),
  egressWorkspaceId: z.string(),
  netappFolderPath: z.string(),
  operationName: z.string(),
  activeTransferId: z.string().nullable(),
  urn: z.string(),
});

export type CaseMetaDataResponse = z.infer<typeof caseMetaDataResponseSchema>;
