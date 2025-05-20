import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  getCaseSearchResults,
  getCaseDivisionsOrAreas,
  connectEgressWorkspace,
  getEgressSearchResults,
  getConnectNetAppFolders,
  connectNetAppFolder,
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
});
