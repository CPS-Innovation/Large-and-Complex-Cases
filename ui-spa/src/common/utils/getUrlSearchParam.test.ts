import { getUrlSearchParam } from "./getUrlSearchParam";

describe("getUrlSearchParam", () => {
  it("returns URLSearchParams with the provided value", () => {
    const params = getUrlSearchParam("workspace-name", "my-workspace");
    expect(params.toString()).toBe("workspace-name=my-workspace");
  });

  it("uses backtick placeholder when value is null", () => {
    const params = getUrlSearchParam("workspace-name", null);
    expect(params.toString()).toBe("workspace-name=%60");
  });

  it("uses backtick placeholder when value is undefined", () => {
    const params = getUrlSearchParam("workspace-name", undefined);
    expect(params.toString()).toBe("workspace-name=%60");
  });

  it("encodes special characters properly in the param value", () => {
    const value = "a space & symbols?";
    const params = getUrlSearchParam("workspace-name", value);
    expect(params.toString()).toBe("workspace-name=a+space+%26+symbols%3F");
  });
});
