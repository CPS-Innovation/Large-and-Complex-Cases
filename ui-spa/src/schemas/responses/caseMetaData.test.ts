import { describe, it, expect } from "vitest";
import { caseMetaDataResponseSchema } from "./caseMetaData";

describe("caseMetaDataResponseSchema", () => {
  const base = {
    caseId: 123,
    egressWorkspaceId: "egress-1",
    netappFolderPath: "\\\netapp\\folder",
    operationName: "Operation A",
    leadDefendantName: "John Doe",
    activeTransferId: null,
    urn: "URN-1",
  };

  it("parses a valid object", () => {
    const result = caseMetaDataResponseSchema.safeParse(base);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.caseId).toBe(123);
      expect(result.data.operationName).toBe("Operation A");
    }
  });

  it("allows operationName to be null when leadDefendantName is present", () => {
    const input = { ...base, operationName: null };
    const result = caseMetaDataResponseSchema.safeParse(input);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.operationName).toBeNull();
      expect(result.data.leadDefendantName).toBe("John Doe");
    }
  });

  it("allows leadDefendantName to be null when operationName is present", () => {
    const input = { ...base, leadDefendantName: null };
    const result = caseMetaDataResponseSchema.safeParse(input);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.leadDefendantName).toBeNull();
      expect(result.data.operationName).toBe("Operation A");
    }
  });

  it("rejects when both operationName and leadDefendantName are null", () => {
    const input = { ...base, operationName: null, leadDefendantName: null };
    const result = caseMetaDataResponseSchema.safeParse(input);
    expect(result.success).toBe(false);
    if (!result.success) {
      const messages = result.error.issues.map((i) => i.message);
      expect(messages).toContain(
        "Both `operationName` and `leadDefendantName` cannot both be null",
      );
    }
  });
});
