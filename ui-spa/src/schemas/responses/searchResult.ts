import { z } from "zod";

export const searchResultSchema = z.object({
  operationName: z.string(),
  urn: z.string(),
  caseId: z.number(),
  leadDefendantName: z.string(),
  egressWorkspaceId: z.string().nullable(),
  netappFolderPath: z.string().nullable(),
  registrationDate: z.string().nullable(),
});

export const searchResultDataSchema = z.array(searchResultSchema);

export type SearchResult = z.infer<typeof searchResultSchema>;
export type SearchResultData = z.infer<typeof searchResultDataSchema>;
