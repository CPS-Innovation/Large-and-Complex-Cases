import { z } from "zod";

export const createEgressCasePayloadSchema = z.object({
  caseId: z.number(),
});

export type CreateEgressCasePayload = z.infer<
  typeof createEgressCasePayloadSchema
>;
