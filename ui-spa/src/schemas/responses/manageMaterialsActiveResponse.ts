import { z } from "zod";

export const manageMaterialsOperationSchema = z.object({
  id: z.string(),
  operationType: z.string(),
  sourcePaths: z.string(),
  destinationPaths: z.string().nullable(),
  userName: z.string().nullable(),
  createdAt: z.string(),
});

export const manageMaterialsActiveResponseSchema = z.array(
  manageMaterialsOperationSchema,
);

export type ManageMaterialsOperation = z.infer<
  typeof manageMaterialsOperationSchema
>;
export type ManageMaterialsActiveResponse = z.infer<
  typeof manageMaterialsActiveResponseSchema
>;
