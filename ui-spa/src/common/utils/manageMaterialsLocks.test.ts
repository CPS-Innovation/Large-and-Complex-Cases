import { describe, it, expect } from "vitest";
import type { ManageMaterialsOperation } from "../../schemas";
import {
  parsePaths,
  pathsOverlap,
  collectLockedPaths,
  isPathLocked,
} from "./manageMaterialsLocks";

describe("parsePaths", () => {
  it("returns an empty array for null/undefined/empty", () => {
    expect(parsePaths(null)).toEqual([]);
    expect(parsePaths(undefined)).toEqual([]);
    expect(parsePaths("")).toEqual([]);
  });

  it("parses a JSON array of paths", () => {
    expect(parsePaths('["/a/", "/b/"]')).toEqual(["/a/", "/b/"]);
  });

  it("returns an empty array for invalid JSON or non-array JSON", () => {
    expect(parsePaths("not json")).toEqual([]);
    expect(parsePaths('{"foo":"bar"}')).toEqual([]);
  });
});

describe("pathsOverlap", () => {
  it("matches identical paths case-insensitively and ignoring trailing slashes", () => {
    expect(pathsOverlap("/case/Interviews/", "/case/interviews")).toBe(true);
  });

  it("matches a parent and child folder", () => {
    expect(pathsOverlap("/case/Interviews/", "/case/Interviews/Sub/")).toBe(
      true,
    );
    expect(pathsOverlap("/case/Interviews/Sub/", "/case/Interviews/")).toBe(
      true,
    );
  });

  it("does not match sibling folders that share a prefix", () => {
    expect(pathsOverlap("/case/Interviews/", "/case/Interviews v2/")).toBe(
      false,
    );
  });

  it("does not match unrelated folders", () => {
    expect(pathsOverlap("/case/Interviews/", "/case/Exhibits/")).toBe(false);
  });
});

describe("collectLockedPaths", () => {
  const makeOp = (
    sourcePaths: string,
    destinationPaths: string | null,
  ): ManageMaterialsOperation => ({
    id: "op-1",
    operationType: "copy",
    sourcePaths,
    destinationPaths,
    userName: null,
    createdAt: "2026-06-15T00:00:00Z",
  });

  it("flattens source and destination paths across operations", () => {
    const ops = [
      makeOp('["/case/A/"]', '["/case/B/"]'),
      makeOp('["/case/C/"]', null),
    ];
    expect(collectLockedPaths(ops)).toEqual(["/case/A/", "/case/B/", "/case/C/"]);
  });

  it("returns an empty array when there are no operations", () => {
    expect(collectLockedPaths([])).toEqual([]);
  });
});

describe("isPathLocked", () => {
  const lockedPaths = ["/case/Interviews/", "/case/Exhibits/Photos/"];

  it("returns true when the path overlaps a locked path", () => {
    expect(isPathLocked("/case/Interviews/Sub/", lockedPaths)).toBe(true);
    expect(isPathLocked("/case/Exhibits/", lockedPaths)).toBe(true);
  });

  it("returns false when the path does not overlap any locked path", () => {
    expect(isPathLocked("/case/Statements/", lockedPaths)).toBe(false);
    expect(isPathLocked("/case/Interviews v2/", lockedPaths)).toBe(false);
  });

  it("returns false when there are no locked paths", () => {
    expect(isPathLocked("/case/Interviews/", [])).toBe(false);
  });
});
