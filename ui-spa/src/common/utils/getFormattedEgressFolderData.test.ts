import { getFormattedEgressFolderData } from "./getFormattedEgressFolderData";
import { EgressFolderData } from "../types/EgressFolderData";

describe("getFormattedEgressFolderData", () => {
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
        id: "3",
        name: "abc3.pdf",
        isFolder: false,
        dateUpdated: "04/10/2000",
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

    const formattedFolderData = getFormattedEgressFolderData(folderData);
    expect(formattedFolderData).toEqual(expectedResult);
  });

  it("Should return the formatted egress folder data sorted with folders first then files", () => {
    const folderData: EgressFolderData = [
      {
        id: "1",
        name: "abc1.pdf",
        isFolder: false,
        dateUpdated: "04/10/2000",
        filesize: 1234,
        path: "",
      },
      {
        id: "2",
        name: "abc",
        isFolder: true,
        dateUpdated: "02/10/2000",
        filesize: 1234,
        path: "",
      },
      {
        id: "3",
        name: "abc3.txt",
        isFolder: false,
        dateUpdated: "05/10/2000",
        filesize: 1234,
        path: "abc/def",
      },
      {
        id: "4",
        name: "abc4",
        isFolder: true,
        dateUpdated: "03/10/2000",
        filesize: 1234,
        path: "abc",
      },

      {
        id: "5",
        name: "abc5",
        isFolder: true,
        dateUpdated: "06/10/2000",
        filesize: 1234,
        path: "abc",
      },
    ];
    const expectedResult: EgressFolderData = [
      {
        id: "2",
        name: "abc",
        isFolder: true,
        dateUpdated: "02/10/2000",
        filesize: 1234,
        path: "abc/",
      },
      {
        id: "4",
        name: "abc4",
        isFolder: true,
        dateUpdated: "03/10/2000",
        filesize: 1234,
        path: "abc/abc4/",
      },
      {
        id: "5",
        name: "abc5",
        isFolder: true,
        dateUpdated: "06/10/2000",
        filesize: 1234,
        path: "abc/abc5/",
      },

      {
        id: "1",
        name: "abc1.pdf",
        isFolder: false,
        dateUpdated: "04/10/2000",
        filesize: 1234,
        path: "abc1.pdf",
      },
      {
        id: "3",
        name: "abc3.txt",
        isFolder: false,
        dateUpdated: "05/10/2000",
        filesize: 1234,
        path: "abc/def/abc3.txt",
      },
    ];

    const formattedFolderData = getFormattedEgressFolderData(folderData);
    expect(formattedFolderData).toEqual(expectedResult);
  });
});
