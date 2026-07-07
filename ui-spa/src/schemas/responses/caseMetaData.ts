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
    // both operationName and leadDefendantName cannot be null at the same time
    if (val.operationName === null && val.leadDefendantName === null) {
      ctx.addIssue(
        "Both `operationName` and `leadDefendantName` cannot both be null",
      );
    }
  });

export type CaseMetaDataResponse = z.infer<typeof caseMetaDataResponseSchema>;
