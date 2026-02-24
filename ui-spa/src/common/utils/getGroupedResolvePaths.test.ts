import {
  ResolvePathFileType,
  getGroupedResolvePaths,
} from "./getGroupedResolvePaths";

describe("getGroupedResolvePaths", () => {
  test("It should map the resolve path files into groups based on the relativeFinalPath", () => {
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
        relativeFinalPath: "destination/folder4",
      },
      {
        id: "5",
        relativeSourcePath: "folder5",
        sourceName: "file5.pdf",
        relativeFinalPath: "destination",
      },
      {
        id: "6",
        relativeSourcePath: "folder6",
        sourceName: "file6.pdf",
        relativeFinalPath: "destination",
      },
      {
        id: "7",
        relativeSourcePath: "folder6",
        sourceName: "file7.pdf",
        relativeFinalPath: "",
      },
    ];
    const expectedResult = {
      "destination/folder1/folder2": [
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
      "destination/folder1/folder2/folder3": [
        {
          id: "3",
          relativeSourcePath: "folder1/folder2/folder3",
          sourceName: "file3.pdf",
          relativeFinalPath: "destination/folder1/folder2/folder3",
        },
      ],
      "destination/folder4": [
        {
          id: "4",
          relativeSourcePath: "folder6",
          sourceName: "file4.pdf",
          relativeFinalPath: "destination/folder4",
        },
      ],
      destination: [
        {
          id: "5",
          relativeSourcePath: "folder5",
          sourceName: "file5.pdf",
          relativeFinalPath: "destination",
        },
        {
          id: "6",
          relativeSourcePath: "folder6",
          sourceName: "file6.pdf",
          relativeFinalPath: "destination",
        },
      ],
      "": [
        {
          id: "7",
          relativeSourcePath: "folder6",
          sourceName: "file7.pdf",
          relativeFinalPath: "",
        },
      ],
    };
    const result = getGroupedResolvePaths(files);
    expect(result).toEqual(expectedResult);
  });
});
