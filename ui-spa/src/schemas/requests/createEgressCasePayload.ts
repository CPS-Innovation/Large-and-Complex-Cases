import { z } from "zod";

export const createEgressCasePayloadSchema = z.object({
  templateId: z.string(),
  caseId: z.number(),
  description: z.string(),
});

export type CreateEgressCasePayload = z.infer<
  typeof createEgressCasePayloadSchema
>;
