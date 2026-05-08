import { z } from "zod";

export const connectNetAppPayloadSchema = z.object({
  operationName: z.string(),
  folderPath: z.string(),
  caseId: z.number(),
});

export type ConnectNetAppPayload = z.infer<typeof connectNetAppPayloadSchema>;
