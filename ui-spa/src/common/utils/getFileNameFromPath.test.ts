import { getFileNameFromPath } from "./getFileNameFromPath";

describe("getFileNameFromPath", () => {
  test("Should return las folder name from the path", () => {
    expect(getFileNameFromPath("abc.json")).toEqual("abc.json");
    expect(getFileNameFromPath("/")).toEqual("");
    expect(getFileNameFromPath("/abc/abc.json")).toEqual("abc.json");
    expect(getFileNameFromPath("/abc/bcd/def/def.json")).toEqual("def.json");
    expect(getFileNameFromPath("/abc/bcd/def/xyz.pdf")).toEqual("xyz.pdf");
    expect(getFileNameFromPath("/abc/bcd/def/xyz/")).toEqual("");
    expect(getFileNameFromPath("netapp/file-0-0.pdf")).toEqual("file-0-0.pdf");
  });
});
