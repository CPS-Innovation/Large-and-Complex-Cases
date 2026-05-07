import { z } from "zod";

const activeManageMaterialsOperationSchema = z.object({
  id: z.string(),
  operationType: z.string(),
  sourcePaths: z.string(),
  destinationPaths: z.string().nullable(),
  userName: z.string().nullable(),
  createdAt: z.string(),
});

export const searchResultSchema = z.object({
  operationName: z.string().nullable(),
  urn: z.string(),
  caseId: z.number(),
  leadDefendantName: z.string(),
  egressWorkspaceId: z.string().nullable(),
  netappFolderPath: z.string().nullable(),
  registrationDate: z.string().nullable(),
  activeTransferId: z.string().nullable(),
  activeManageMaterialsOperations: z.array(activeManageMaterialsOperationSchema),
});

export const searchResultDataSchema = z.array(searchResultSchema);

export type SearchResult = z.infer<typeof searchResultSchema>;
export type SearchResultData = z.infer<typeof searchResultDataSchema>;
