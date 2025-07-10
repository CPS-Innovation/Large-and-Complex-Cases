import {
  ResolvePathFileType,
  getGroupedResolvePaths,
} from "./getGroupedResolvePaths";

describe("getGroupedResolvePaths", () => {
  test("It should map the resolve path files into groups based on their relatvie path", () => {
    const files: ResolvePathFileType[] = [
      {
        id: "1",
        relativeSourcePath: "folder1/folder2",
        sourceName: "file1.pdf",
        relativeFinalPath: "destination/folder1/folder2",
      },
      {
        id: "2",
        relativeSourcePath: "folder1/folder2",
        sourceName: "file2.pdf",
        relativeFinalPath: "destination/folder1/folder2",
      },
      {
        id: "3",
        relativeSourcePath: "folder1/folder2/folder3",
        sourceName: "file3.pdf",
        relativeFinalPath: "destination/folder1/folder2/folder3",
      },
      {
        id: "4",
        relativeSourcePath: "folder6",
        sourceName: "file4.pdf",
        relativeFinalPath: "destination/folder6",
      },
    ];
    const expectedResult = {
      "folder1/folder2": [
        {
          id: "1",
          relativeSourcePath: "folder1/folder2",
          sourceName: "file1.pdf",
          relativeFinalPath: "destination/folder1/folder2",
        },
        {
          id: "2",
          relativeSourcePath: "folder1/folder2",
          sourceName: "file2.pdf",
          relativeFinalPath: "destination/folder1/folder2",
        },
      ],
      "folder1/folder2/folder3": [
        {
          id: "3",
          relativeSourcePath: "folder1/folder2/folder3",
          sourceName: "file3.pdf",
          relativeFinalPath: "destination/folder1/folder2/folder3",
        },
      ],
      folder6: [
        {
          id: "4",
          relativeSourcePath: "folder6",
          sourceName: "file4.pdf",
          relativeFinalPath: "destination/folder6",
        },
      ],
    };
    const result = getGroupedResolvePaths(files, "base-path-abc-1");
    expect(result).toEqual(expectedResult);
  });
  test("It should use the base path name for grouping if the relative path is empty", () => {
    const files: ResolvePathFileType[] = [
      {
        id: "1",
        relativeSourcePath: "",
        sourceName: "file1.pdf",
        relativeFinalPath: "destination",
      },
      {
        id: "2",
        relativeSourcePath: "",
        sourceName: "file2.pdf",
        relativeFinalPath: "destination",
      },
      {
        id: "3",
        relativeSourcePath: "folder1/folder2/folder3",
        sourceName: "file3.pdf",
        relativeFinalPath: "destination/folder1/folder2/folder3",
      },
      {
        id: "4",
        relativeSourcePath: "folder6",
        sourceName: "file4.pdf",
        relativeFinalPath: "destination/folder6",
      },
    ];
    const expectedResult = {
      "base-path-abc-1": [
        {
          id: "1",
          relativeSourcePath: "",
          sourceName: "file1.pdf",
          relativeFinalPath: "destination",
        },
        {
          id: "2",
          relativeSourcePath: "",
          sourceName: "file2.pdf",
          relativeFinalPath: "destination",
        },
      ],
      "folder1/folder2/folder3": [
        {
          id: "3",
          relativeSourcePath: "folder1/folder2/folder3",
          sourceName: "file3.pdf",
          relativeFinalPath: "destination/folder1/folder2/folder3",
        },
      ],
      folder6: [
        {
          id: "4",
          relativeSourcePath: "folder6",
          sourceName: "file4.pdf",
          relativeFinalPath: "destination/folder6",
        },
      ],
    };
    const result = getGroupedResolvePaths(files, "base-path-abc-1");
    expect(result).toEqual(expectedResult);
  });
});
