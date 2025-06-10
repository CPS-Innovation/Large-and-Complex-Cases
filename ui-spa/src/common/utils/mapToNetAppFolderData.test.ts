import { mapToNetAppFolderData } from "./mapToNetAppFolderData";
import {
  NetAppFolderDataResponse,
  NetAppFolderData,
} from "../types/NetAppFolderData";

describe("mapToNetAppFolderData", () => {
  test("Should successfully map netapp response data into NetAppFolderData format", () => {
    const responseData: NetAppFolderDataResponse = {
      fileData: [
        {
          path: "netapp/file-1-0.pdf",
          lastModified: "2000-01-02",
          filesize: 1234,
        },
        {
          path: "netapp/file-1-0.pdf",
          lastModified: "2000-01-03",
          filesize: 2268979,
        },
      ],
      folderData: [
        {
          path: "netapp/folder-1-0/",
        },
        {
          path: "netapp/folder-1-1/",
        },

        {
          path: "netapp/folder-1-2/",
        },
      ],
    };

    const expectedResult: NetAppFolderData = [
      {
        path: "netapp/folder-1-0/",
        filesize: 0,
        lastModified: "",
        isFolder: true,
      },
      {
        path: "netapp/folder-1-1/",
        filesize: 0,
        lastModified: "",
        isFolder: true,
      },
      {
        path: "netapp/folder-1-2/",
        filesize: 0,
        lastModified: "",
        isFolder: true,
      },
      {
        path: "netapp/file-1-0.pdf",
        lastModified: "2000-01-02",
        filesize: 1234,
        isFolder: false,
      },
      {
        path: "netapp/file-1-0.pdf",
        lastModified: "2000-01-03",
        filesize: 2268979,
        isFolder: false,
      },
    ];

    const result = mapToNetAppFolderData(responseData);
    expect(result).toEqual(expectedResult);
  });
});
