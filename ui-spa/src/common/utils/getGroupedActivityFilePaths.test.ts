import { getGroupedActvityFilePaths } from "./getGroupedActivityFilePaths";

describe("getGroupedActvityFilePaths", () => {
  test(
    "should group the successFiles and failedFiles inot relevant relative path groups",
  );
  const successFiles = [
    { path: "abc/folder1/file1.pdf" },
    { path: "abc/folder1/file2.pdf" },
    { path: "abc/folder1/folder2/file3.pdf" },
    { path: "abc/folder1/folder2/folder3/file3.pdf" },
    { path: "abc/folder1/folder2/folder3/file4.pdf" },
    { path: "abc/file1.pdf" },
    { path: "abc/folder3/file4.pdf" },
  ];
  const failedFiles = [
    { path: "abc/folder1/file6.pdf" },
    { path: "abc/folder1/folder2/file7.pdf" },
    { path: "abc/folder1/folder2/file8.pdf" },
    { path: "abc/folder1/folder2/folder3/file9.pdf" },
    { path: "abc/file9.pdf" },
    { path: "abc/folder5/file11.pdf" },
  ];
  const sourcePath = "abc";

  const expectedResult = {
    "": {
      success: [{ fileName: "file1.pdf" }],
      errors: [{ fileName: "file9.pdf" }],
    },
    folder1: {
      success: [{ fileName: "file1.pdf" }, { fileName: "file2.pdf" }],
      errors: [{ fileName: "file6.pdf" }],
    },
    "folder1 > folder2": {
      success: [{ fileName: "file3.pdf" }],
      errors: [{ fileName: "file7.pdf" }, { fileName: "file8.pdf" }],
    },
    "folder1 > folder2 > folder3": {
      success: [{ fileName: "file3.pdf" }, { fileName: "file4.pdf" }],
      errors: [{ fileName: "file9.pdf" }],
    },
    folder3: {
      success: [{ fileName: "file4.pdf" }],
      errors: [],
    },
    folder5: {
      success: [],
      errors: [{ fileName: "file11.pdf" }],
    },
  };

  expect(
    getGroupedActvityFilePaths(successFiles, failedFiles, sourcePath),
  ).toStrictEqual(expectedResult);
});
