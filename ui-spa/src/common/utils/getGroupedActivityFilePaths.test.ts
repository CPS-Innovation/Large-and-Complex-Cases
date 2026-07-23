import { getGroupedActivityFilePaths } from "./getGroupedActivityFilePaths";

describe("getGroupedActivityFilePaths", () => {
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
        errors: [{ fileName: "file9.pdf" }],
        skipped: [],
        success: [{ fileName: "file1.pdf" }],
      },
      folder1: {
        errors: [{ fileName: "file6.pdf" }],
        skipped: [],
        success: [{ fileName: "file1.pdf" }, { fileName: "file2.pdf" }],
      },
      "folder1 > folder2": {
        errors: [{ fileName: "file7.pdf" }, { fileName: "file8.pdf" }],
        skipped: [],
        success: [{ fileName: "file3.pdf" }],
      },
      "folder1 > folder2 > folder3": {
        errors: [{ fileName: "file9.pdf" }],
        skipped: [],
        success: [{ fileName: "file3.pdf" }, { fileName: "file4.pdf" }],
      },
      folder3: {
        errors: [],
        skipped: [],
        success: [{ fileName: "file4.pdf" }],
      },
      folder5: {
        errors: [{ fileName: "file11.pdf" }],
        skipped: [],
        success: [],
      },
    };

    expect(
      getGroupedActivityFilePaths(successFiles, failedFiles, sourcePath),
    ).toStrictEqual(expectedResult);
  });

  test("should preserve folder hierarchy when failed paths include the same source root as success paths", () => {
    const successFiles = [
      { path: "2. Investigation/folder1/subfolder/file1.txt" },
    ];
    const failedFiles = [
      { path: "2. Investigation/folder1/subfolder/file2.txt" },
      { path: "2. Investigation/folder1/subfolder/file3.txt" },
    ];
    const sourcePath = "2. Investigation/";

    const expectedResult = {
      "folder1 > subfolder": {
        errors: [{ fileName: "file2.txt" }, { fileName: "file3.txt" }],
        skipped: [],
        success: [{ fileName: "file1.txt" }],
      },
    };

    expect(
      getGroupedActivityFilePaths(successFiles, failedFiles, sourcePath),
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
        errors: [],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file1.txt" },
        ],
      },
      "abc.1": {
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file2.txt" }],
        skipped: [],
        success: [],
      },
    };

    expect(
      getGroupedActivityFilePaths(successFiles, failedFiles, sourcePath),
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
        errors: [],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file1.txt" },
        ],
      },
      "4. Served Evidence": {
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file5.txt" }],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file2.txt" },
        ],
      },
      "4. Served Evidence > abc3": {
        errors: [],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file7.txt" },
        ],
      },
      "Served Evidence2 > abc3": {
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file6.txt" }],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file3.txt" },
        ],
      },
    };

    expect(
      getGroupedActivityFilePaths(successFiles, failedFiles, sourcePath),
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
        errors: [],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file1.txt" },
        ],
      },
      "4. Served Evidence": {
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file5.txt" }],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file2.txt" },
        ],
      },
      "4. Served Evidence > abc3": {
        errors: [],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file7.txt" },
        ],
      },
      "Served Evidence2 > abc3": {
        errors: [{ fileName: "generated-100MB-2026-02-12-01-41-46-file6.txt" }],
        skipped: [],
        success: [
          { fileName: "generated-100MB-2026-02-12-01-41-46-file3.txt" },
        ],
      },
    };

    expect(
      getGroupedActivityFilePaths(successFiles, failedFiles, sourcePath),
    ).toStrictEqual(expectedResult);
  });

  test("should group skipped files with a Skipped outcome", () => {
    const successFiles = [{ path: "abc/file1.pdf" }];
    const failedFiles: { path: string }[] = [];
    const skippedFiles = [{ path: "abc/empty.txt" }];
    const sourcePath = "abc";

    expect(
      getGroupedActivityFilePaths(
        successFiles,
        failedFiles,
        sourcePath,
        skippedFiles,
      ),
    ).toStrictEqual({
      "": {
        errors: [],
        skipped: [{ fileName: "empty.txt" }],
        success: [{ fileName: "file1.pdf" }],
      },
    });
  });
});
