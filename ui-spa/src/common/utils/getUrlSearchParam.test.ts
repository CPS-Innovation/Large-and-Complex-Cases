import { getUrlSearchParam } from "./getUrlSearchParam";

describe("getUrlSearchParam", () => {
  it("returns URLSearchParams with the provided value", () => {
    const params = getUrlSearchParam("workspace-name", "my-workspace");
    expect(params.toString()).toBe("workspace-name=my-workspace");
  });

  it("should return correctly when value is null", () => {
    const params = getUrlSearchParam("workspace-name", null);
    expect(params.toString()).toBe("workspace-name=");
  });

  it("should return correctly when value is undefined", () => {
    const params = getUrlSearchParam("workspace-name", undefined);
    expect(params.toString()).toBe("workspace-name=");
  });

  it("encodes special characters properly in the param value", () => {
    const value = "a space & symbols?";
    const params = getUrlSearchParam("workspace-name", value);
    expect(params.toString()).toBe("workspace-name=a+space+%26+symbols%3F");
  });
});
