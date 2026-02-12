import { getGroupedActvityFilePaths } from "./getGroupedActivityFilePaths";

describe("getGroupedActvityFilePaths", () => {
  test("should group the successFiles and failedFiles into relevant relative path groups", () => {
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

  test("should group the successFiles and failedFiles into relevant relative path groups", () => {
    const successFiles = [
      {
        path: "4. Served Evidence/generated-100MB-2026-02-12-01-41-46-file1.txt",
      },
    ];
    const failedFiles = [
      {
        path: "4. Served Evidence/abc.1/generated-100MB-2026-02-12-01-41-46-file2.txt",
      },
    ];
    const sourcePath = "4. Served Evidence";

    const expectedResult = {
      "": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file1.txt" },
        ],
        errors: [],
      },
      "abc.1": {
        success: [],
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file2.txt" }],
      },
    };

    expect(
      getGroupedActvityFilePaths(successFiles, failedFiles, sourcePath),
    ).toStrictEqual(expectedResult);
  });

  test("should group the successFiles and failedFiles into relevant relative path groups with single source folder path has multiple levels", () => {
    const successFiles = [
      {
        path: "1 .  investigation/2 . test/3 .multiple/generated-100MB-2026-02-12-01-41-46-file1.txt",
      },
      {
        path: "1 .  investigation/2 . test/3 .multiple/4. Served Evidence/generated-100MB-2026-02-12-01-41-46-file2.txt",
      },

      {
        path: "1 .  investigation/2 . test/3 .multiple/Served Evidence2/abc3/generated-100MB-2026-02-12-01-41-46-file3.txt",
      },
      {
        path: "1 .  investigation/2 . test/3 .multiple/4. Served Evidence/abc3/generated-100MB-2026-02-12-01-41-46-file7.txt",
      },
    ];
    const failedFiles = [
      {
        path: "1 .  investigation/2 . test/3 .multiple/4. Served Evidence/generated-100MB-2026-02-12-01-41-46-file5.txt",
      },
      {
        path: "1 .  investigation/2 . test/3 .multiple/Served Evidence2/abc3/generated-100MB-2026-02-12-01-41-46-file6.txt",
      },
    ];
    const sourcePath = "1 .  investigation/2 . test/3 .multiple/";

    const expectedResult = {
      "": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file1.txt" },
        ],
        errors: [],
      },
      "4. Served Evidence": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file2.txt" },
        ],
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file5.txt" }],
      },
      "4. Served Evidence > abc3": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file7.txt" },
        ],
        errors: [],
      },
      "Served Evidence2 > abc3": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file3.txt" },
        ],
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file6.txt" }],
      },
    };

    expect(
      getGroupedActvityFilePaths(successFiles, failedFiles, sourcePath),
    ).toStrictEqual(expectedResult);
  });

  test("should group the successFiles and failedFiles into relevant relative path groups with single source folder is empty", () => {
    const successFiles = [
      {
        path: "generated-100MB-2026-02-12-01-41-46-file1.txt",
      },
      {
        path: "4. Served Evidence/generated-100MB-2026-02-12-01-41-46-file2.txt",
      },

      {
        path: "Served Evidence2/abc3/generated-100MB-2026-02-12-01-41-46-file3.txt",
      },
      {
        path: "4. Served Evidence/abc3/generated-100MB-2026-02-12-01-41-46-file7.txt",
      },
    ];
    const failedFiles = [
      {
        path: "4. Served Evidence/generated-100MB-2026-02-12-01-41-46-file5.txt",
      },
      {
        path: "Served Evidence2/abc3/generated-100MB-2026-02-12-01-41-46-file6.txt",
      },
    ];
    const sourcePath = "";

    const expectedResult = {
      "": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file1.txt" },
        ],
        errors: [],
      },
      "4. Served Evidence": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file2.txt" },
        ],
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file5.txt" }],
      },
      "4. Served Evidence > abc3": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file7.txt" },
        ],
        errors: [],
      },
      "Served Evidence2 > abc3": {
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file3.txt" },
        ],
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file6.txt" }],
      },
    };

    expect(
      getGroupedActvityFilePaths(successFiles, failedFiles, sourcePath),
    ).toStrictEqual(expectedResult);
  });
});
