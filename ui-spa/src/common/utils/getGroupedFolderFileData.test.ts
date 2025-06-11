import { getGroupedFolderFileData } from "./getGroupedFolderFileData";
import { EgressFolderData } from "../types/EgressFolderData";
import { NetAppFolderData } from "../types/NetAppFolderData";

describe("getGroupedFolderFileData", () => {
  test("It should group the selected Egress data into folders and files", () => {
    const egressData: EgressFolderData = [
      {
        id: "1",
        name: "abc",
        isFolder: true,
        dateUpdated: "02/01/2000",
        filesize: 0,
        path: "ab/def/abc",
      },
      {
        id: "2",
        name: "abc1",
        isFolder: true,
        dateUpdated: "03/01/2000",
        filesize: 0,
        path: "ab/def/abc1",
      },
      {
        id: "3",
        name: "abc2",
        isFolder: true,
        dateUpdated: "04/01/2000",
        filesize: 0,
        path: "ab/def/abc2",
      },
      {
        id: "4",
        name: "pqr.pdf",
        isFolder: false,
        dateUpdated: "03/01/2000",
        filesize: 100,
        path: "ab/def/pqr.pdf",
      },
      {
        id: "5",
        name: "pqr1.pdf",
        isFolder: false,
        dateUpdated: "04/01/2000",
        filesize: 100,
        path: "ab/def/pqr1.pdf",
      },
    ];
    const selectedIds = ["ab/def/abc", "ab/def/abc1", "ab/def/pqr.pdf"];
    const expectedResult = {
      folders: ["ab/def/abc", "ab/def/abc1"],
      files: ["ab/def/pqr.pdf"],
    };
    const result = getGroupedFolderFileData(selectedIds, egressData);
    expect(result).toEqual(expectedResult);
  });
  test("It should group the selected Netapp data into folders and files", () => {
    const egressData: NetAppFolderData = [
      {
        isFolder: true,
        lastModified: "02/01/2000",
        filesize: 0,
        path: "ab/def/abc",
      },
      {
        isFolder: true,
        lastModified: "03/01/2000",
        filesize: 0,
        path: "ab/def/abc1",
      },
      {
        isFolder: true,
        lastModified: "04/01/2000",
        filesize: 0,
        path: "ab/def/abc2",
      },
      {
        isFolder: false,
        lastModified: "03/01/2000",
        filesize: 100,
        path: "ab/def/pqr.pdf",
      },
      {
        isFolder: false,
        lastModified: "04/01/2000",
        filesize: 100,
        path: "ab/def/pqr1.pdf",
      },
    ];
    const selectedIds = ["ab/def/abc", "ab/def/abc1", "ab/def/pqr.pdf"];
    const expectedResult = {
      folders: ["ab/def/abc", "ab/def/abc1"],
      files: ["ab/def/pqr.pdf"],
    };
    const result = getGroupedFolderFileData(selectedIds, egressData);
    expect(result).toEqual(expectedResult);
  });
});
