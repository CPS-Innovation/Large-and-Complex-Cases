import { getMappedResolvePathFiles } from "./getMappedResolvePathFiles";
import { type IndexingError } from "../../schemas";
import { ResolvePathFileType } from "./getGroupedResolvePaths";

describe("getMappedResolvePathFiles", () => {
  it("Should correctly map indexing errors into ResolvePathFileType", () => {
    const indexingErrors: IndexingError[] = [
      {
        id: "1",
        sourcePath: "folder1/file1.pdf",
        destinationFullPath: "destination/folder1/file1.pdf",
      },
      {
        id: "2",
        sourcePath: "folder1/file2.pdf",
        destinationFullPath: "destination/folder1/file2.pdf",
      },
      {
        id: "3",
        sourcePath: "folder1/folder2/file3.pdf",
        destinationFullPath: "destination/folder1/folder2/file3.pdf",
      },
      {
        id: "4",
        sourcePath: "file4.pdf",
        destinationFullPath: "destination/file4.pdf",
      },
    ];

    const expectedResult: ResolvePathFileType[] = [
      {
        id: "1",
        relativeSourcePath: "folder1",
        sourceName: "file1.pdf",
        relativeFinalPath: "destination/folder1/",
      },
      {
        id: "2",
        relativeSourcePath: "folder1",
        sourceName: "file2.pdf",
        relativeFinalPath: "destination/folder1/",
      },
      {
        id: "3",
        relativeSourcePath: "folder1/folder2",
        sourceName: "file3.pdf",
        relativeFinalPath: "destination/folder1/folder2/",
      },
      {
        id: "4",
        relativeSourcePath: "",
        sourceName: "file4.pdf",
        relativeFinalPath: "destination/",
      },
    ];

    const result = getMappedResolvePathFiles(indexingErrors);
    expect(result).toEqual(expectedResult);
  });

  it("Should use destinationFullPath for display path length", () => {
    const longDirectory =
      "\\\\cps-fileshare\\netapp01\\Area Shares\\CCU Manchester\\Op Milton\\1. Incoming Master Copy\\17. Egress 160726\\Billing data for all suspects\\Billing data\\" +
      "a".repeat(100);
    const fileName = "CP_0000_TEST_billing_summary.xlsx";
    const destinationFullPath = `${longDirectory}\\${fileName}`;

    const indexingErrors: IndexingError[] = [
      {
        id: "1",
        sourcePath: `Billing data/${fileName}`,
        destinationFullPath,
      },
    ];

    const result = getMappedResolvePathFiles(indexingErrors);
    const normalizedFullPath = destinationFullPath.replace(/\\/g, "/");
    const expectedFinalPath = `${normalizedFullPath.slice(0, normalizedFullPath.lastIndexOf("/") + 1)}`;

    expect(result).toEqual([
      {
        id: "1",
        relativeSourcePath: "Billing data",
        sourceName: fileName,
        relativeFinalPath: expectedFinalPath,
      },
    ]);

    const fullPathLength = `${result[0].relativeFinalPath}${result[0].sourceName}`
      .length;
    expect(fullPathLength).toBeGreaterThan(260);
    expect(fullPathLength).toBe(normalizedFullPath.length);
  });
});
