import { sortRelativePaths } from "./sortRelativePaths";

describe("sortRelativePaths", () => {
  it("Should sort relative paths in ascending order of the number of child folders seperated using '>' character", () => {
    const relativePaths = [
      "abc",
      "abc>folder1",
      "abc>folder1>folder2",
      "",
      "abc>folder1>folder2>folder3>folder4",
      "abc>folder5",
      "abc>folder6>folder8",
    ];

    const sortedPaths = [
      "",
      "abc",
      "abc>folder1",
      "abc>folder5",
      "abc>folder1>folder2",
      "abc>folder6>folder8",
      "abc>folder1>folder2>folder3>folder4",
    ];

    expect(sortRelativePaths(relativePaths)).toEqual(sortedPaths);
  });
});
