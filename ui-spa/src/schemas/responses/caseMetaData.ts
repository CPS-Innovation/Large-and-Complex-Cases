import { z } from "zod";
import { manageMaterialsOperationSchema } from "./manageMaterialsActiveResponse";

export const caseMetaDataResponseSchema = z.object({
  caseId: z.number(),
  egressWorkspaceId: z.string(),
  netappFolderPath: z.string(),
  operationName: z.string(),
  activeTransferId: z.string().nullable(),
  urn: z.string(),
  activeManageMaterialsOperations: z.array(manageMaterialsOperationSchema).default([]),
});

export type CaseMetaDataResponse = z.infer<typeof caseMetaDataResponseSchema>;
