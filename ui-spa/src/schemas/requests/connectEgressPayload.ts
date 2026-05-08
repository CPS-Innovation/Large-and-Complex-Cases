import { z } from "zod";

export const connectEgressPayloadSchema = z.object({
  egressWorkspaceId: z.string(),
  caseId: z.number(),
});

export type ConnectEgressPayload = z.infer<typeof connectEgressPayloadSchema>;
