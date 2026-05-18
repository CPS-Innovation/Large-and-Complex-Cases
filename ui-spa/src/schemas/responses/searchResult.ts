import { z } from "zod";

export const searchResultSchema = z
  .object({
    operationName: z.string().nullable(),
    urn: z.string(),
    caseId: z.number(),
    leadDefendantName: z.string().nullable(),
    egressWorkspaceId: z.string().nullable(),
    netappFolderPath: z.string().nullable(),
    registrationDate: z.string().nullable(),
  })
  .superRefine((val, ctx) => {
    // both operationName and leadDefendantName cannot be null at the same time
    if (val.operationName === null && val.leadDefendantName === null) {
      ctx.addIssue(
        "Both `operationName` and `leadDefendantName` cannot both be null",
      );
    }
  });

export const searchResultDataSchema = z.array(searchResultSchema);

export type SearchResult = z.infer<typeof searchResultSchema>;
export type SearchResultData = z.infer<typeof searchResultDataSchema>;
