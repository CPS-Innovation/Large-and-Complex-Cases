import { z } from "zod";

export const InitiateFileTransferResponseSchema = z.object({
  id: z.string(),
});

export type InitiateFileTransferResponse = z.infer<
  typeof InitiateFileTransferResponseSchema
>;
