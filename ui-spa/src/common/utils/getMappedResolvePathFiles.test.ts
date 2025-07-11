import { getMappedResolvePathFiles } from "./getMappedResolvePathFiles";
import { IndexingError } from "../types/IndexingFileTransferResponse";
import { ResolvePathFileType } from "./getGroupedResolvePaths";

describe("getMappedResolvePathFiles", () => {
  it("Should correctly map indexing errors into ResolvePathFileType", () => {
    const indexingErrors: IndexingError[] = [
      {
        id: "1",
        sourcePath: "folder1/file1.pdf",
      },
      {
        id: "2",
        sourcePath: "folder1/file2.pdf",
      },
      {
        id: "3",
        sourcePath: "folder1/folder2/file3.pdf",
      },
      {
        id: "4",
        sourcePath: "file4.pdf",
      },
    ];

    const expectedResult: ResolvePathFileType[] = [
      {
        id: "1",
        relativeSourcePath: "folder1",
        sourceName: "file1.pdf",
        relativeFinalPath: "destination/folder1",
      },
      {
        id: "2",
        relativeSourcePath: "folder1",
        sourceName: "file2.pdf",
        relativeFinalPath: "destination/folder1",
      },
      {
        id: "3",
        relativeSourcePath: "folder1/folder2",
        sourceName: "file3.pdf",
        relativeFinalPath: "destination/folder1/folder2",
      },
      {
        id: "4",
        relativeSourcePath: "",
        sourceName: "file4.pdf",
        relativeFinalPath: "destination",
      },
    ];

    const result = getMappedResolvePathFiles(indexingErrors, "destination");
    expect(result).toEqual(expectedResult);
  });
});
