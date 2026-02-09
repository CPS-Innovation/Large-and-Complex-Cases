import { expect, test, vi, afterEach, beforeEach } from "vitest";
import { getDuplicateFoldersAndFiles } from "./getDuplicateFoldersAndFiles";
import { type EgressFolderData } from "../types/EgressFolderData";
import { type NetAppFolderData } from "../types/NetAppFolderData";
import * as groupedModule from "./getGroupedFolderFileData";
import { type TransferAction } from "../types/TransferAction";

describe("getDuplicateFoldersAndFiles", () => {
  let groupedSpy: any;

  beforeEach(() => {
    groupedSpy = vi.spyOn(groupedModule, "getGroupedFolderFileData");
  });

  afterEach(() => {
    groupedSpy?.mockRestore();
  });

  test("returns empty folders/files when destination doesn't match roots for egress to netapp transfer", () => {
    const egressData: EgressFolderData = [
      {
        id: "1",
        name: "f1",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f1/",
      },
    ];
    const netappData: NetAppFolderData = [
      {
        path: "netapp/f1/",
        isFolder: true,
        lastModified: "2020-01-01",
        filesize: 0,
      },
    ];

    const transferAction = {
      destinationFolder: { path: "some/other/path/" },
    } as TransferAction;

    const result = getDuplicateFoldersAndFiles(
      "egress",
      transferAction,
      egressData,
      netappData,
      ["egress/f1/"],
      "egress/",
      "netapp/",
    );

    expect(result).toEqual({ folders: [], files: [] });
    expect(groupedSpy).not.toHaveBeenCalled();
  });

  test("returns empty folders/files when destination doesn't match roots for netapp to egress transfer", () => {
    const egressData: EgressFolderData = [
      {
        id: "1",
        name: "f1",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f1/",
      },
    ];
    const netappData: NetAppFolderData = [
      {
        path: "netapp/f1/",
        isFolder: true,
        lastModified: "2020-01-01",
        filesize: 0,
      },
    ];

    const transferAction = {
      destinationFolder: { path: "some/other/path/" },
    } as TransferAction;

    const result = getDuplicateFoldersAndFiles(
      "netapp",
      transferAction,
      egressData,
      netappData,
      ["netapp/f1/"],
      "egress/",
      "netapp/",
    );

    expect(result).toEqual({ folders: [], files: [] });
    expect(groupedSpy).not.toHaveBeenCalled();
  });

  test("detects duplicates when transferring from egress into netapp (egress -> netapp)", () => {
    const egressData: EgressFolderData = [
      {
        id: "1",
        name: "f1",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f1/",
      },
      {
        id: "2",
        name: "f2",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f2/",
      },
      {
        id: "3",
        name: "file.pdf",
        isFolder: false,
        dateUpdated: "2020-01-02",
        filesize: 10,
        path: "egress/file.pdf",
      },
    ];

    const netappData: NetAppFolderData = [
      {
        path: "netapp/f1/",
        isFolder: true,
        lastModified: "2020-01-01",
        filesize: 0,
      },
      {
        path: "netapp/other/",
        isFolder: true,
        lastModified: "2020-01-01",
        filesize: 0,
      },
      {
        path: "netapp/file.pdf",
        isFolder: false,
        lastModified: "2020-01-01",
        filesize: 0,
      },
    ];

    const transferAction = { destinationFolder: { path: "netapp/" } } as any;

    const result = getDuplicateFoldersAndFiles(
      "egress",
      transferAction,
      egressData,
      netappData,
      ["egress/f1/", "egress/file.pdf"],
      "egress/",
      "netapp/",
    );

    expect(result).toEqual({
      folders: ["egress/f1/"],
      files: ["egress/file.pdf"],
    });
    expect(groupedSpy).toHaveBeenCalledTimes(1);
    expect(groupedSpy).toHaveBeenCalledWith(
      ["egress/f1/", "egress/file.pdf"],
      egressData,
    );
  });

  test("detects duplicates when transferring from netapp into egress (netapp -> egress)", () => {
    const egressData: EgressFolderData = [
      {
        id: "1",
        name: "f1",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f1/",
      },
      {
        id: "2",
        name: "file1.pdf",
        isFolder: false,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/file1.pdf",
      },
      {
        id: "3",
        name: "f2",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f2/",
      },
    ];

    const netappData: NetAppFolderData = [
      {
        path: "netapp/f1/",
        isFolder: true,
        lastModified: "2020-01-01",
        filesize: 0,
      },
      {
        path: "netapp/file1.pdf",
        isFolder: false,
        lastModified: "2020-01-02",
        filesize: 5,
      },
    ];

    const transferAction = { destinationFolder: { path: "egress/" } } as any;

    const result = getDuplicateFoldersAndFiles(
      "netapp",
      transferAction,
      egressData,
      netappData,
      ["netapp/f1/", "netapp/file1.pdf"],
      "egress/",
      "netapp/",
    );

    expect(result).toEqual({
      folders: ["netapp/f1/"],
      files: ["netapp/file1.pdf"],
    });
    expect(groupedSpy).toHaveBeenCalledTimes(1);
    expect(groupedSpy).toHaveBeenCalledWith(
      ["netapp/f1/", "netapp/file1.pdf"],
      netappData,
    );
  });

  test("detects duplicates when there are for longer paths (egress -> netapp)", () => {
    const egressData: EgressFolderData = [
      {
        id: "1",
        name: "f1",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f1/f2/f3/f1/",
      },
      {
        id: "2",
        name: "f2",
        isFolder: true,
        dateUpdated: "2020-01-01",
        filesize: 0,
        path: "egress/f1/f2/f3/f2/",
      },
      {
        id: "3",
        name: "file.pdf",
        isFolder: false,
        dateUpdated: "2020-01-02",
        filesize: 10,
        path: "egress/f1/f2/f3/file.pdf",
      },
    ];

    const netappData: NetAppFolderData = [
      {
        path: "netapp/f1/",
        isFolder: true,
        lastModified: "2020-01-01",
        filesize: 0,
      },
      {
        path: "netapp/other/",
        isFolder: true,
        lastModified: "2020-01-01",
        filesize: 0,
      },
      {
        path: "netapp/file.pdf",
        isFolder: false,
        lastModified: "2020-01-01",
        filesize: 0,
      },
    ];

    const transferAction = { destinationFolder: { path: "netapp/" } } as any;

    const result = getDuplicateFoldersAndFiles(
      "egress",
      transferAction,
      egressData,
      netappData,
      ["egress/f1/f2/f3/f1/", "egress/f1/f2/f3/file.pdf"],
      "egress/f1/f2/f3/",
      "netapp/",
    );

    expect(result).toEqual({
      folders: ["egress/f1/f2/f3/f1/"],
      files: ["egress/f1/f2/f3/file.pdf"],
    });
    expect(groupedSpy).toHaveBeenCalledTimes(1);
    expect(groupedSpy).toHaveBeenCalledWith(
      ["egress/f1/f2/f3/f1/", "egress/f1/f2/f3/file.pdf"],
      egressData,
    );
  });
});
