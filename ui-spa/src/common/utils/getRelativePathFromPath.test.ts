import { getRelativePathFromPath } from "./getRelativePathFromPath";
describe("getRelativePathFromPath", () => {
  test("Should return the relative path from the path if there is match", () => {
    expect(getRelativePathFromPath("abc/def/a.pdf")).toEqual("abc/def");
    expect(getRelativePathFromPath("abc/adc")).toEqual("abc");
    expect(getRelativePathFromPath("abc/adc/def/jjj")).toEqual("abc/adc/def");
    expect(getRelativePathFromPath("abc/def/")).toEqual("abc");
  });
  test("Should return empty string if there is no match", () => {
    expect(getRelativePathFromPath("abc/def//")).toEqual("");
    expect(getRelativePathFromPath("abc")).toEqual("");
    expect(getRelativePathFromPath("")).toEqual("");
  });
});
