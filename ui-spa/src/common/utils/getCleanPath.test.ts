import { getCleanPath } from "./getCleanPath";

describe("getCleanPath", () => {
  it("should remove any trailing '/' from the path name", () => {
    expect(getCleanPath("abc/folder1/")).toStrictEqual("abc/folder1");
    expect(getCleanPath("abc/folder1/////")).toStrictEqual("abc/folder1");
    expect(getCleanPath("abc/folder1")).toStrictEqual("abc/folder1");
  });
});
