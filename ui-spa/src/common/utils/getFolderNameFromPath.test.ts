import { getFolderNameFromPath } from "./getFolderNameFromPath";

describe("getFolderNameFromPath", () => {
  test("Should return las folder name from the path", () => {
    expect(getFolderNameFromPath("/")).toEqual("");
    expect(getFolderNameFromPath("/abc")).toEqual("abc");
    expect(getFolderNameFromPath("/abc/bcd/def")).toEqual("def");
    expect(getFolderNameFromPath("/abc/bcd/def/")).toEqual("");
    expect(getFolderNameFromPath("/abc/bcd/def/xyz")).toEqual("xyz");
  });
});
