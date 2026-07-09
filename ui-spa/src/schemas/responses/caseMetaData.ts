import { z } from "zod";

export const caseMetaDataResponseSchema = z
  .object({
    caseId: z.number(),
    egressWorkspaceId: z.string(),
    netappFolderPath: z.string(),
    operationName: z.string().nullable(),
    leadDefendantName: z.string().nullable(),
    activeTransferId: z.string().nullable(),
    urn: z.string(),
  })
  .superRefine((val, ctx) => {
    // both operationName and leadDefendantName cannot be falsy at the same time
    if (!val.operationName && !val.leadDefendantName) {
      ctx.addIssue(
        "At least one of operationName or leadDefendantName must be provided (they cannot both be empty or null).",
      );
    }
  });

export type CaseMetaDataResponse = z.infer<typeof caseMetaDataResponseSchema>;
