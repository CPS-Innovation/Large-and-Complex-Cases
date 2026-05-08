import { z } from "zod";

export const egressSearchResultSchema = z.object({
  dateCreated: z.string(),
  id: z.string(),
  name: z.string(),
  caseId: z.number().nullable(),
});

export const egressSearchResultDataSchema = z.array(egressSearchResultSchema);

export const egressSearchResultResponseSchema = z.object({
  data: egressSearchResultDataSchema,
  pagination: z.object({
    totalResults: z.number(),
    skip: z.number(),
    take: z.number(),
    count: z.number(),
  }),
});

export type EgressSearchResult = z.infer<typeof egressSearchResultSchema>;
export type EgressSearchResultData = z.infer<
  typeof egressSearchResultDataSchema
>;
export type EgressSearchResultResponse = z.infer<
  typeof egressSearchResultResponseSchema
>;
