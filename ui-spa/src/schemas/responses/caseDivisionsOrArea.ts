import { z } from "zod";

export const caseDivisionsOrAreaSchema = z.object({
  id: z.number(),
  description: z.string(),
  default: z.boolean().optional(),
});

export const caseDivisionsOrAreaResponseSchema = z.object({
  allAreas: z.array(caseDivisionsOrAreaSchema),
  userAreas: z.array(caseDivisionsOrAreaSchema),
  homeArea: caseDivisionsOrAreaSchema,
});

export type CaseDivisionsOrArea = z.infer<typeof caseDivisionsOrAreaSchema>;

export type CaseDivisionsOrAreaResponse = z.infer<
  typeof caseDivisionsOrAreaResponseSchema
>;
