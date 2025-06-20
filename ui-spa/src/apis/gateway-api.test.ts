import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  getCaseSearchResults,
  getCaseDivisionsOrAreas,
  connectEgressWorkspace,
  getEgressSearchResults,
  getConnectNetAppFolders,
  connectNetAppFolder,
  getCaseMetaData,
  getEgressFolders,
  getNetAppFolders,
  validateFileTransfer,
  initiateFileTransfer,
  getTransferStatus,
} from "./gateway-api";
import { ApiError } from "../common/errors/ApiError";
import { v4 } from "uuid";
import { getAccessToken } from "../auth";

vi.mock("uuid", () => ({
  v4: vi.fn(),
}));

vi.mock("../auth", () => ({
  getAccessToken: vi.fn(),
}));

vi.mock("../config", () => ({
  GATEWAY_BASE_URL: "gateway_url",
  GATEWAY_SCOPE: "gateway_scope",
}));

describe("gateway apis", () => {
  global.fetch = vi.fn();

  beforeEach(() => {
    vi.resetAllMocks();
  });

  describe("getCaseSearchResults", () => {
    it("should return case search data when fetch is successful", async () => {
      const mockData = [
        {
          operationName: "abc",
          urn: "urn",
          caseId: "123",
          leadDefendantName: "abc",
          egressWorkspaceId: "abcdef",
          netappFolderPath: null,
          registrationDate: "null",
        },
      ];
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getCaseSearchResults("thunder");
      expect(result).toEqual(mockData);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/case-search?thunder`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(getCaseSearchResults("thunder")).rejects.toThrow(ApiError);

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/case-search?thunder`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("getCaseDivisionsOrAreas", () => {
    it("should return area data when fetch is successful", async () => {
      const mockData = {
        allAreas: [{ id: 1, description: "Area A" }],
        userAreas: [{ id: 2, description: "Area B" }],
        homeArea: [{ id: 2, description: "Area B" }],
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getCaseDivisionsOrAreas();
      expect(result).toEqual(mockData);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/areas`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(getCaseDivisionsOrAreas()).rejects.toThrow(
        new ApiError(`Getting case areas failed`, "gateway_url/api/areas", {
          status: 500,
          statusText: "Internal Server Error",
        }),
      );
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/areas`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("getEgressSearchResults", () => {
    it("should return egress search data when fetch is successful", async () => {
      const mockData = {
        data: [],
        pagination: {
          totalResults: 50,
          skip: 0,
          take: 50,
          count: 25,
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getEgressSearchResults("thunder", 0, 50, []);
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/egress/workspaces?thunder&skip=0&take=50`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should make multiple calls to get the paginated data and then successfully return the data", async () => {
      const mockData = {
        data: [],
        pagination: {
          totalResults: 100,
          skip: 0,
          take: 50,
          count: 25,
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getEgressSearchResults("thunder", 0, 50, []);
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(fetch).toHaveBeenNthCalledWith(
        1,
        `gateway_url/api/egress/workspaces?thunder&skip=0&take=50`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
      expect(fetch).toHaveBeenNthCalledWith(
        2,
        `gateway_url/api/egress/workspaces?thunder&skip=50&take=50`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(
        getEgressSearchResults("thunder", 0, 50, []),
      ).rejects.toThrow(
        new ApiError(
          `Searching for Egress workspaces failed`,
          "gateway_url/api/egress/workspaces",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/egress/workspaces?thunder&skip=0&take=50`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw Error if the response data is not in correct format", async () => {
      const mockData = {
        data: [],
        pagination1: {
          totalResults: 100,
          skip: 0,
          take: 50,
          count: 25,
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      await expect(
        getEgressSearchResults("thunder", 0, 50, []),
      ).rejects.toThrow(
        "Invalid API response format for Egress workspace search results",
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/egress/workspaces?thunder&skip=0&take=50`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("connectEgressWorkspace", () => {
    it("should return success response if the post request is successful", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
      });

      const result = await connectEgressWorkspace({
        workspaceId: "thunder_1",
        caseId: "123",
      });
      expect(result).toEqual({ ok: true });
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/egress/connections`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify({
            egressWorkspaceId: "thunder_1",
            caseId: 123,
          }),
        }),
      );
    });

    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(
        connectEgressWorkspace({
          workspaceId: "thunder_1",
          caseId: "123",
        }),
      ).rejects.toThrow(ApiError);

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/egress/connections`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify({
            egressWorkspaceId: "thunder_1",
            caseId: 123,
          }),
        }),
      );
    });
  });
  describe("getConnectNetAppFolders", () => {
    it("should return netapp connect data folders when fetch is successful", async () => {
      const mockData = {
        data: {
          rootPath: "netapp/",
          folders: [],
        },
        pagination: {
          nextContinuationToken: "",
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getConnectNetAppFolders(
        "thunder",
        "/netapp",
        50,
        "",
        [],
      );
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/folders?operation-name=thunder&path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
    it("should make multiple calls to get the paginated data and then successfully return the data", async () => {
      const mockData1 = {
        data: { rootPath: "netapp/", folders: [{ id: 1 }] },
        pagination: {
          nextContinuationToken: "abc",
        },
      };
      const mockData2 = {
        data: { rootPath: "netapp/", folders: [{ id: 2 }, { id: 3 }] },
        pagination: {
          nextContinuationToken: "",
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any)
        .mockResolvedValueOnce({
          ok: true,
          json: async () => mockData1,
        })
        .mockResolvedValue({ ok: true, json: async () => mockData2 });

      const result = await getConnectNetAppFolders(
        "thunder",
        "/netapp",
        50,
        "",
        [],
      );
      expect(result).toEqual({
        rootPath: "netapp/",
        folders: [{ id: 1 }, { id: 2 }, { id: 3 }],
      });
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(fetch).toHaveBeenNthCalledWith(
        1,
        `gateway_url/api/netapp/folders?operation-name=thunder&path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
      expect(fetch).toHaveBeenNthCalledWith(
        2,
        `gateway_url/api/netapp/folders?operation-name=thunder&path=/netapp&take=50&continuation-token=abc`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(
        getConnectNetAppFolders("thunder", "/netapp", 50, "", []),
      ).rejects.toThrow(
        new ApiError(
          `getting netapp folders failed`,
          "gateway_url/api/netapp/folders",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/folders?operation-name=thunder&path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
    it("should throw Error if the response data is not in correct format", async () => {
      const mockData = {
        data: [],
        pagination1: {
          nextContinuationToken: "",
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      await expect(
        getConnectNetAppFolders("thunder", "/netapp", 50, "", []),
      ).rejects.toThrow(
        "Invalid API response format for netapp folders results",
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/folders?operation-name=thunder&path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("connectNetAppFolder", () => {
    it("should return success response if the post request is successful", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
      });

      const result = await connectNetAppFolder({
        operationName: "thunder",
        folderPath: "netapp/",
        caseId: "123",
      });
      expect(result).toEqual({ ok: true });
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/connections`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify({
            operationName: "thunder",
            folderPath: "netapp/",
            caseId: 123,
          }),
        }),
      );
    });

    it("should throw an ApiError when post request fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });
      await expect(
        connectNetAppFolder({
          operationName: "thunder",
          folderPath: "netapp",
          caseId: "123",
        }),
      ).rejects.toThrow(
        new ApiError(
          `Connecting to NetApp folder failed`,
          `gateway_url/api/netapp/connections`,
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/connections`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify({
            operationName: "thunder",
            folderPath: "netapp",
            caseId: 123,
          }),
        }),
      );
    });
  });
  describe("getCaseMetaData", () => {
    it("should return case meta data when fetch is successful", async () => {
      const mockData = {
        caseId: "12",
        egressWorkspaceId: "egress_1",
        netappFolderPath: "netapp/",
        operationName: "Thunderstruck",
        urn: "45AA2098221",
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getCaseMetaData("12");
      expect(result).toEqual(mockData);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/cases/12`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(getCaseMetaData("12")).rejects.toThrow(
        new ApiError(
          `Getting case metadata failed`,
          "gateway_url/api/cases/12",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/cases/12`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("getEgressFolders", () => {
    it("should return egress folders when fetch is successful", async () => {
      const mockData = {
        data: [],
        pagination: {
          totalResults: 50,
          skip: 0,
          take: 50,
          count: 25,
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getEgressFolders("thunder", "folder-1", 0, 50, []);
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledWith(
        "gateway_url/api/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50",
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should make multiple calls to get the paginated data and then successfully return the data", async () => {
      const mockData = {
        data: [],
        pagination: {
          totalResults: 100,
          skip: 0,
          take: 50,
          count: 25,
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getEgressFolders("thunder", "folder-1", 0, 50, []);
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(fetch).toHaveBeenNthCalledWith(
        1,
        "gateway_url/api/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50",
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
      expect(fetch).toHaveBeenNthCalledWith(
        2,
        "gateway_url/api/egress/workspaces/thunder/files?folder-id=folder-1&skip=50&take=50",
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(
        getEgressFolders("thunder", "folder-1", 0, 50, []),
      ).rejects.toThrow(
        new ApiError(
          `Getting egress folders failed`,
          "gateway_url/api/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw Error if the response data is not in correct format", async () => {
      const mockData = {
        data: [],
        pagination1: {
          totalResults: 100,
          skip: 0,
          take: 50,
          count: 25,
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      await expect(
        getEgressFolders("thunder", "folder-1", 0, 50, []),
      ).rejects.toThrow("Invalid API response format for Egress folders");

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("getNetAppFolders", () => {
    it("should return netapp folders when fetch is successful", async () => {
      const mockData = {
        data: {
          fileData: [],
          folderData: [],
        },
        pagination: {
          nextContinuationToken: "",
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getNetAppFolders("/netapp", 50, "", []);
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/files?path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
    it("should make multiple calls to get the paginated data and then successfully return the data", async () => {
      const mockData1 = {
        data: {
          fileData: [{ id: 1 }],
          folderData: [{ id: 1 }],
        },
        pagination: {
          nextContinuationToken: "abc",
        },
      };
      const mockData2 = {
        data: {
          fileData: [{ id: 2 }, { id: 3 }],
          folderData: [{ id: 2 }, { id: 3 }],
        },
        pagination: {
          nextContinuationToken: "",
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any)
        .mockResolvedValueOnce({
          ok: true,
          json: async () => mockData1,
        })
        .mockResolvedValue({ ok: true, json: async () => mockData2 });

      const result = await getNetAppFolders("/netapp", 50, "", []);
      expect(result).toEqual({
        fileData: [{ id: 1 }, { id: 2 }, { id: 3 }],
        folderData: [{ id: 1 }, { id: 2 }, { id: 3 }],
      });
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(fetch).toHaveBeenNthCalledWith(
        1,
        `gateway_url/api/netapp/files?path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
      expect(fetch).toHaveBeenNthCalledWith(
        2,
        `gateway_url/api/netapp/files?path=/netapp&take=50&continuation-token=abc`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(getNetAppFolders("/netapp", 50, "", [])).rejects.toThrow(
        new ApiError(
          `getting netapp files/folders failed`,
          "gateway_url/api/netapp/files",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/files?path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
    it("should throw Error if the response data is not in correct format", async () => {
      const mockData = {
        data: {
          fileData: [],
          folderData: [],
        },
        pagination1: {
          nextContinuationToken: "",
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      await expect(getNetAppFolders("/netapp", 50, "", [])).rejects.toThrow(
        "Invalid API response format for netapp files/folders results",
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/netapp/files?path=/netapp&take=50&continuation-token=`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("validateFileTransfer", () => {
    it("should successfully call the post request and return the response", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: () => ({
          caseId: 12,
        }),
      });

      const payload = {
        caseId: "12",
        transferType: "COPY" as const,
        direction: "EgressToNetApp" as const,
        sourcePaths: [
          {
            id: "1",
            path: "abc/def",
          },
        ],
        destinationBasePath: "netapp/",
      };
      const result = await validateFileTransfer(payload);
      expect(result).toEqual({ caseId: 12 });
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/validate`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify(payload),
        }),
      );
    });

    it("should throw an ApiError when post request fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });
      const payload = {
        caseId: "12",
        transferType: "COPY" as const,
        direction: "EgressToNetApp" as const,
        sourcePaths: [
          {
            id: "1",
            path: "abc/def",
          },
        ],
        destinationBasePath: "netapp/",
      };

      await expect(validateFileTransfer(payload)).rejects.toThrow(
        new ApiError(
          `validating file transfer failed`,
          `gateway_url/api/v1/filetransfer/validate`,
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/validate`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify(payload),
        }),
      );
    });
  });
  describe("initiateFileTransfer", () => {
    it("should successfully call the post request and return the response", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: () => ({
          transferId: "12",
        }),
      });

      const payload = {
        isRetry: false,
        caseId: "12",
        transferType: "COPY" as const,
        direction: "EgressToNetApp" as const,
        sourcePaths: [],
        destinationPath: "netapp/",
      };
      const result = await initiateFileTransfer(payload);
      expect(result).toEqual({ transferId: "12" });
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/initiate`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify(payload),
        }),
      );
    });

    it("should throw an ApiError when post request fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });
      const payload = {
        isRetry: false,
        caseId: "12",
        transferType: "COPY" as const,
        direction: "EgressToNetApp" as const,
        sourcePaths: [],
        destinationPath: "netapp/",
      };

      await expect(initiateFileTransfer(payload)).rejects.toThrow(
        new ApiError(
          `initiate file transfer failed`,
          `gateway_url/api/v1/filetransfer/initiate`,
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/initiate`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
          body: JSON.stringify(payload),
        }),
      );
    });
  });
  describe("getTransferStatus", () => {
    it("should return transferStatus when fetch is successful", async () => {
      const mockData = {
        caseId: 12,
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getTransferStatus("transfer_id_1");
      expect(result).toEqual(mockData);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/transfer_id_1/status`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(getTransferStatus("transfer_id_1")).rejects.toThrow(
        new ApiError(
          `Getting case transfer status failed`,
          "gateway_url/api/v1/filetransfer/transfer_id_1/status",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/transfer_id_1/status`,
        expect.objectContaining({
          method: "GET",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
});
