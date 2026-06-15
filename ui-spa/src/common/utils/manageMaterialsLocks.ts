import type { ManageMaterialsOperation } from "../../schemas";

export const parsePaths = (jsonStr: string | null | undefined): string[] => {
  if (!jsonStr) return [];
  try {
    const parsed = JSON.parse(jsonStr);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
};

// Matches the backend rules: case-insensitive, trailing slashes ignored,
// parent and child folders count as the same area. Comparison is on whole
// path segments so siblings like "Interviews" and "Interviews v2" don't clash.
export const pathsOverlap = (a: string, b: string): boolean => {
  const la = a.toLowerCase().replace(/\/+$/, "");
  const lb = b.toLowerCase().replace(/\/+$/, "");
  return la === lb || la.startsWith(`${lb}/`) || lb.startsWith(`${la}/`);
};

export const collectLockedPaths = (
  ops: ManageMaterialsOperation[],
): string[] =>
  ops.flatMap((op) => [
    ...parsePaths(op.sourcePaths),
    ...parsePaths(op.destinationPaths),
  ]);

export const isPathLocked = (path: string, lockedPaths: string[]): boolean =>
  lockedPaths.some((lp) => pathsOverlap(path, lp));
