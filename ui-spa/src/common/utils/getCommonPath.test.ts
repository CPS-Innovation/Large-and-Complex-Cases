import { getCommonPath } from "./getCommonPath";

describe("getCommonPath", () => {
  it("Should return the common path from a set of paths", () => {
    expect(
      getCommonPath([
        "abc/def/def/ggg/lmn",
        "abc/def/erf/ggg/lmn",
        "abc/def/def1/ggg/kkk/lmn",
      ]),
    ).toEqual("abc/def/");
    expect(
      getCommonPath([
        "abc/def/def/ggg/lmn",
        "abc/def/erf/ggg/lmn",
        "abc/def/def1/ggg/kkk/lmn",
        "abc/hh",
      ]),
    ).toEqual("abc/");
    expect(
      getCommonPath([
        "abc/def/def/ggg/lmn",
        "abc/def/def/ggg/kk",
        "abc/def/def/ggg/kkk/lmn",
      ]),
    ).toEqual("abc/def/def/ggg/");
  });

  it("Should return the relative path if there is only one single path", () => {
    expect(getCommonPath(["abc/def/def/ggg/lmn"])).toEqual("abc/def/def/ggg/");
  });
});
