import { getFormatedEgressFolderData } from "./getFormatedEgressFolderData";
import { EgressFolderData } from "../types/EgressFolderData";

describe("getFormatedEgressFolderData", () => {
  it("Should return the formatted egress folder data with new path which appends file and folder name to the end", () => {
    const folderData: EgressFolderData = [
      {
        id: "1",
        name: "abc",
        isFolder: true,
        dateUpdated: "02/10/2000",
        filesize: 1234,
        path: "",
      },
      {
        id: "2",
        name: "abc2",
        isFolder: true,
        dateUpdated: "03/10/2000",
        filesize: 1234,
        path: "abc",
      },
      {
        id: "3",
        name: "abc3.pdf",
        isFolder: false,
        dateUpdated: "04/10/2000",
        filesize: 1234,
        path: "",
      },
      {
        id: "4",
        name: "abc4.txt",
        isFolder: false,
        dateUpdated: "05/10/2000",
        filesize: 1234,
        path: "abc/def",
      },
    ];
    const expectedResult: EgressFolderData = [
      {
        id: "1",
        name: "abc",
        isFolder: true,
        dateUpdated: "02/10/2000",
        filesize: 1234,
        path: "abc/",
      },
      {
        id: "2",
        name: "abc2",
        isFolder: true,
        dateUpdated: "03/10/2000",
        filesize: 1234,
        path: "abc/abc2/",
      },
      {
        id: "3",
        name: "abc3.pdf",
        isFolder: false,
        dateUpdated: "04/10/2000",
        filesize: 1234,
        path: "abc3.pdf",
      },
      {
        id: "4",
        name: "abc4.txt",
        isFolder: false,
        dateUpdated: "05/10/2000",
        filesize: 1234,
        path: "abc/def/abc4.txt",
      },
    ];

    const formatedFolderData = getFormatedEgressFolderData(folderData);
    expect(formatedFolderData).toEqual(expectedResult);
  });
});
