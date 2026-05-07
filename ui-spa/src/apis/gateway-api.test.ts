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
  indexingFileTransfer,
  initiateFileTransfer,
  getTransferStatus,
  handleFileTransferClear,
  getActivityLog,
  downloadActivityLog,
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
    it("getCaseSearchResults - should return case search data when fetch is successful", async () => {
      const mockData = [
        {
          operationName: "abc",
          urn: "urn",
          caseId: 123,
          leadDefendantName: "abc",
          egressWorkspaceId: "abcdef",
          netappFolderPath: null,
          registrationDate: null,
        },
      ];
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getCaseSearchResults({
        "defendant-name": "husband&wife",
        area: "10",
      });
      expect(result).toEqual(mockData);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/case-search?defendant-name=husband%26wife&area=10`,
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

    it("getCaseSearchResults - should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(
        getCaseSearchResults({ "defendant-name": "husband&wife", area: "10" }),
      ).rejects.toThrow(ApiError);

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/case-search?defendant-name=husband%26wife&area=10`,
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

    it("getCaseSearchResults - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(
        getCaseSearchResults({ "defendant-name": "husband&wife", area: "10" }),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getCaseSearchResults({ "defendant-name": "husband&wife", area: "10" }),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/case-search?defendant-name=husband%26wife&area=10: SyntaxError: Unexpected token < in JSON",
      );
    });
    it("getCaseSearchResults - response schema validation failed", async () => {
      const mockData = [
        {
          operationName: "abc",
          urn: "urn",
          caseId: "123",
          leadDefendantName: "abc",
          egressWorkspaceId: "abcdef",
          netappFolderPath: null,
          registrationDate: null,
        },
      ];
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(
        getCaseSearchResults({ "defendant-name": "husband&wife", area: "10" }),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getCaseSearchResults({ "defendant-name": "husband&wife", area: "10" }),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/case-search?defendant-name=husband%26wife&area=10: response schema validation failed; status - OK (200)",
      );
    });
  });

  describe("getCaseDivisionsOrAreas", () => {
    it("getCaseDivisionsOrAreas- should return area data when fetch is successful", async () => {
      const mockData = {
        allAreas: [{ id: 1, description: "Area A" }],
        userAreas: [{ id: 2, description: "Area B" }],
        homeArea: { id: 2, description: "Area B" },
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
        `gateway_url/api/v1/areas`,
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

    it("getCaseDivisionsOrAreas- should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(getCaseDivisionsOrAreas()).rejects.toThrow(
        new ApiError(`Getting case areas failed`, "gateway_url/api/v1/areas", {
          status: 500,
          statusText: "Internal Server Error",
        }),
      );
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/areas`,
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
    it("getCaseDivisionsOrAreas - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(getCaseDivisionsOrAreas()).rejects.toBeInstanceOf(ApiError);
      await expect(getCaseDivisionsOrAreas()).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/areas: SyntaxError: Unexpected token < in JSON; status - OK (200)",
      );
    });
    it("getCaseDivisionsOrAreas - response schema validation failed", async () => {
      const mockData = {
        allAreas: [{ id: 1, description: "Area A" }],
        userAreas: [{ id: 2, description: "Area B" }],
        homeArea: [{ id: 2, description: "Area B" }],
      };
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(getCaseDivisionsOrAreas()).rejects.toBeInstanceOf(ApiError);
      await expect(getCaseDivisionsOrAreas()).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/areas: response schema validation failed; status - OK (200)",
      );
    });
  });
  describe("getEgressSearchResults", () => {
    it("getEgressSearchResults - should return egress search data when fetch is successful", async () => {
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
        `gateway_url/api/v1/egress/workspaces?workspace-name=thunder&skip=0&take=50`,
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

    it("getEgressSearchResults - should make multiple calls to get the paginated data and then successfully return the data", async () => {
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
        `gateway_url/api/v1/egress/workspaces?workspace-name=thunder&skip=0&take=50`,
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
        `gateway_url/api/v1/egress/workspaces?workspace-name=thunder&skip=50&take=50`,
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

    it("getEgressSearchResults - should throw an ApiError when fetch fails", async () => {
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
          "gateway_url/api/v1/egress/workspaces",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        "gateway_url/api/v1/egress/workspaces?workspace-name=thunder&skip=0&take=50",
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
    it("getEgressSearchResults - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(
        getEgressSearchResults("thunder", 0, 50, []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getEgressSearchResults("thunder", 0, 50, []),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/egress/workspaces: SyntaxError: Unexpected token < in JSON",
      );
    });
    it("getEgressSearchResults -  response schema validation failed", async () => {
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
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(
        getEgressSearchResults("thunder", 0, 50, []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getEgressSearchResults("thunder", 0, 50, []),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/egress/workspaces: response schema validation failed; status - OK (200)",
      );
    });
  });
  describe("connectEgressWorkspace", () => {
    it("connectEgressWorkspace - should return success response if the post request is successful", async () => {
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
        `gateway_url/api/v1/egress/connections`,
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

    it("connectEgressWorkspace - should throw an ApiError when fetch fails", async () => {
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
        `gateway_url/api/v1/egress/connections`,
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

    it("connectEgressWorkspace - should not call fetch and throw Error and console.warn when request schema validation fails ", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
      });

      const payload = {
        workspaceId: 123,
        caseId: "123",
      };

      const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});

      await expect(connectEgressWorkspace(payload as any)).rejects.toThrow(
        new Error(`Invalid connect Egress workspace request payload`),
      );
      expect(fetch).not.toHaveBeenCalled();
      expect(warnSpy).toHaveBeenCalledOnce();
      expect(warnSpy).toHaveBeenCalledWith(
        expect.stringMatching(
          /^Invalid connect Egress workspace request payload:/,
        ),
      );

      warnSpy.mockRestore();
    });
  });
  describe("getConnectNetAppFolders", () => {
    it("getConnectNetAppFolders - should return netapp connect data folders when fetch is successful", async () => {
      const mockData = {
        data: {
          rootPath: "netapp/",
          folders: [],
        },
        pagination: {
          maxKeys: 100,
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
        "thunder&struck",
        "/netapp/folder&test",
        50,
        "",
        [],
      );
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/netapp/folders?operation-name=thunder%26struck&path=%2Fnetapp%2Ffolder%26test&take=50&continuation-token=`,
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
    it("getConnectNetAppFolders -should make multiple calls to get the paginated data and then successfully return the data", async () => {
      const mockData1 = {
        data: {
          rootPath: "netapp/",
          folders: [{ folderPath: "thunderstrikeab/", caseId: 123 }],
        },

        pagination: {
          maxKeys: 100,
          nextContinuationToken: "abc",
        },
      };
      const mockData2 = {
        data: {
          rootPath: "netapp/",
          folders: [{ folderPath: "thunderstrikeacb/", caseId: 1243 }],
        },
        pagination: {
          maxKeys: 100,
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
        folders: [
          { folderPath: "thunderstrikeab/", caseId: 123 },
          { folderPath: "thunderstrikeacb/", caseId: 1243 },
        ],
      });
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(fetch).toHaveBeenNthCalledWith(
        1,
        "gateway_url/api/v1/netapp/folders?operation-name=thunder&path=%2Fnetapp&take=50&continuation-token=",
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
        "gateway_url/api/v1/netapp/folders?operation-name=thunder&path=%2Fnetapp&take=50&continuation-token=abc",
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
    it("getConnectNetAppFolders - should throw an ApiError when fetch fails", async () => {
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
          "gateway_url/api/v1/netapp/folders",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        "gateway_url/api/v1/netapp/folders?operation-name=thunder&path=%2Fnetapp&take=50&continuation-token=",
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
    it("getConnectNetAppFolders - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(
        getConnectNetAppFolders("thunder", "/netapp", 50, "", []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getConnectNetAppFolders("thunder", "/netapp", 50, "", []),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/netapp/folders: SyntaxError: Unexpected token < in JSON",
      );
    });
    it("getConnectNetAppFolders -  response schema validation failed", async () => {
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
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(
        getConnectNetAppFolders("thunder", "/netapp", 50, "", []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getConnectNetAppFolders("thunder", "/netapp", 50, "", []),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/netapp/folders: response schema validation failed; status - OK (200)",
      );
    });
  });
  describe("connectNetAppFolder", () => {
    it("connectNetAppFolder- should return success response if the post request is successful", async () => {
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
        `gateway_url/api/v1/netapp/connections`,
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

    it("connectNetAppFolder- should throw an ApiError when post request fails", async () => {
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
          `gateway_url/api/v1/netapp/connections`,
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/netapp/connections`,
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

    it("connectNetAppFolder - should not call fetch and throw Error and console.warn when request schema validation fails ", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
      });
      const payload = {
        operationName: "thunder",
        folderPath: 123,
        caseId: "123",
      };

      const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});

      await expect(connectNetAppFolder(payload as any)).rejects.toThrow(
        new Error(`Invalid connect Netapp request payload`),
      );
      expect(fetch).not.toHaveBeenCalled();
      expect(warnSpy).toHaveBeenCalledOnce();
      expect(warnSpy).toHaveBeenCalledWith(
        expect.stringMatching(/^Invalid connect Netapp request payload:/),
      );

      warnSpy.mockRestore();
    });
  });
  describe("getCaseMetaData", () => {
    it("getCaseMetaData - should return case meta data when fetch is successful", async () => {
      const mockData = {
        caseId: 12,
        egressWorkspaceId: "egress_1",
        netappFolderPath: "netapp/",
        operationName: "Thunderstruck",
        urn: "45AA2098221",
        activeTransferId: "",
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
        `gateway_url/api/v1/cases/12`,
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

    it("getCaseMetaData - should throw an ApiError when fetch fails", async () => {
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
          "gateway_url/api/v1/cases/12",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/cases/12`,
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

    it("getCaseMetaData - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(getCaseMetaData("12")).rejects.toBeInstanceOf(ApiError);
      await expect(getCaseMetaData("12")).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/cases/12: SyntaxError: Unexpected token < in JSON",
      );
    });

    it("getCaseMetaData -  response schema validation failed", async () => {
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
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(getCaseMetaData("12")).rejects.toBeInstanceOf(ApiError);
      await expect(getCaseMetaData("12")).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/cases/12: response schema validation failed; status - OK (200)",
      );
    });
  });
  describe("getEgressFolders", () => {
    it("getEgressFolders - should return egress folders when fetch is successful", async () => {
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
        "gateway_url/api/v1/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50",
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

    it("getEgressFolders - should make multiple calls to get the paginated data and then successfully return the data", async () => {
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
        "gateway_url/api/v1/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50",
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
        "gateway_url/api/v1/egress/workspaces/thunder/files?folder-id=folder-1&skip=50&take=50",
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

    it("getEgressFolders - should throw an ApiError when fetch fails", async () => {
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
          "gateway_url/api/v1/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50`,
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

    it("getEgressFolders - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(
        getEgressFolders("thunder", "folder-1", 0, 50, []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getEgressFolders("thunder", "folder-1", 0, 50, []),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50: SyntaxError: Unexpected token < in JSON",
      );
    });

    it("getEgressFolders -  response schema validation failed", async () => {
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
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(
        getEgressFolders("thunder", "folder-1", 0, 50, []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(
        getEgressFolders("thunder", "folder-1", 0, 50, []),
      ).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/egress/workspaces/thunder/files?folder-id=folder-1&skip=0&take=50: response schema validation failed; status - OK (200)",
      );
    });
  });
  describe("getNetAppFolders", () => {
    it("getNetAppFolders - should return netapp folders when fetch is successful", async () => {
      const mockData = {
        data: {
          fileData: [
            {
              path: "netapp/file-1-0.pdf",
              lastModified: "2000-01-02",
              filesize: 1234,
            },
            {
              path: "eweweweweewweewwewewewewewewweewerwrrwwrwrrrrrrwrwrwrwweewweeewweweeweweweew.pdf",
              lastModified: "2000-01-03",
              filesize: 2268979,
            },
          ],
          folderData: [
            {
              path: "netapp/folder-1-0/",
            },
          ],
        },

        pagination: {
          maxKeys: 100,
          nextContinuationToken: null,
        },
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getNetAppFolders("/netapp/folder&test", 50, "", []);
      expect(result).toEqual(mockData.data);
      expect(fetch).toHaveBeenCalledWith(
        "gateway_url/api/v1/netapp/files?path=%2Fnetapp%2Ffolder%26test&take=50&continuation-token=",
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
    it("getNetAppFolders - should make multiple calls to get the paginated data and then successfully return the data", async () => {
      const mockData1 = {
        data: {
          fileData: [
            {
              path: "netapp/file-1-0.pdf",
              lastModified: "2000-01-02",
              filesize: 1234,
            },
            {
              path: "ewewewf.pdf",
              lastModified: "2000-01-03",
              filesize: 2268979,
            },
          ],
          folderData: [
            {
              path: "netapp/folder-1-0/",
            },
          ],
        },

        pagination: {
          maxKeys: 10,
          nextContinuationToken: "abc",
        },
      };

      const mockData2 = {
        data: {
          fileData: [
            {
              path: "netapp/file-2-0.pdf",
              lastModified: "2000-01-02",
              filesize: 1234,
            },
            {
              path: "abc.pdf",
              lastModified: "2000-01-03",
              filesize: 2268979,
            },
          ],
          folderData: [
            {
              path: "netapp/folder-2-0/",
            },
          ],
        },

        pagination: {
          maxKeys: 10,
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
        fileData: [
          {
            path: "netapp/file-1-0.pdf",
            lastModified: "2000-01-02",
            filesize: 1234,
          },
          {
            path: "ewewewf.pdf",
            lastModified: "2000-01-03",
            filesize: 2268979,
          },
          {
            path: "netapp/file-2-0.pdf",
            lastModified: "2000-01-02",
            filesize: 1234,
          },
          {
            path: "abc.pdf",
            lastModified: "2000-01-03",
            filesize: 2268979,
          },
        ],
        folderData: [
          {
            path: "netapp/folder-1-0/",
          },
          {
            path: "netapp/folder-2-0/",
          },
        ],
      });
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(fetch).toHaveBeenNthCalledWith(
        1,
        "gateway_url/api/v1/netapp/files?path=%2Fnetapp&take=50&continuation-token=",
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
        "gateway_url/api/v1/netapp/files?path=%2Fnetapp&take=50&continuation-token=abc",
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
    it("getNetAppFolders - should throw an ApiError when fetch fails", async () => {
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
          "gateway_url/api/v1/netapp/files",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        "gateway_url/api/v1/netapp/files?path=%2Fnetapp&take=50&continuation-token=",
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
    it("getNetAppFolders - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(
        getNetAppFolders("/netapp", 50, "", []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(getNetAppFolders("/netapp", 50, "", [])).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/netapp/files: SyntaxError: Unexpected token < in JSON; status - OK (200)",
      );
    });
    it("getNetAppFolders -  response schema validation failed", async () => {
      const mockData = {
        data: {
          fileData1: [
            {
              path: "netapp/file-1-0.pdf",
              lastModified: "2000-01-02",
              filesize: 1234,
            },
            {
              path: "ewewewf.pdf",
              lastModified: "2000-01-03",
              filesize: 2268979,
            },
          ],
          folderData: [
            {
              path: "netapp/folder-1-0/",
            },
          ],
        },

        pagination: {
          maxKeys: 10,
          nextContinuationToken: "",
        },
      };

      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(
        getNetAppFolders("/netapp", 50, "", []),
      ).rejects.toBeInstanceOf(ApiError);
      await expect(getNetAppFolders("/netapp", 50, "", [])).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/netapp/files: response schema validation failed; status - OK (200)",
      );
    });
  });
  describe("indexingFileTransfer", () => {
    it("indexingFileTransfer - should successfully call the post request and return the response", async () => {
      const mockResponse = {
        caseId: 12,
        isInvalid: false,
        destinationPath: "abc/",
        validationErrors: [],
        sourceRootFolderPath: "netapp/",
        transferDirection: "NetAppToEgress",
        files: [
          {
            id: null,
            sourcePath: "netapp/folder1/file1.pdf",
            relativePath: "file1.pdf",
            fullFilePath: null,
          },
          {
            id: null,
            sourcePath: "netapp/folder1/folder2/file2.pdf",
            relativePath: "file2.pdf",
            fullFilePath: null,
          },
        ],
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        json: () => mockResponse,
      });

      const payload = {
        caseId: 12,
        transferDirection: "EgressToNetApp" as const,
        transferType: "Copy" as const,
        sourcePaths: [
          {
            fileId: "1",
            path: "abc/def",
            isFolder: true,
          },
        ],
        sourceRootFolderPath: "abc/",
        destinationPath: "netapp/",
      };
      const result = await indexingFileTransfer(payload);
      expect(result).toEqual(mockResponse);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/files`,
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

    it("indexingFileTransfer - should throw an ApiError when post request fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });
      const payload = {
        caseId: 12,
        transferDirection: "EgressToNetApp" as const,
        transferType: "Copy" as const,
        sourcePaths: [
          {
            fileId: "1",
            path: "abc/def",
            isFolder: true,
          },
        ],
        sourceRootFolderPath: "abc/",
        destinationPath: "netapp/",
      };

      await expect(indexingFileTransfer(payload)).rejects.toThrow(
        new ApiError(
          `indexing file transfer api failed`,
          `gateway_url/api/v1/filetransfer/files`,
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/files`,
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

    it("indexingFileTransfer -  response schema validation failed", async () => {
      const mockResponse = {
        caseId: 12,
        isInvalid: false,
        destinationPath: "abc/",
        validationErrors: [],
        sourceRootFolderPath: "netapp/",
        transferDirection: "NetAppToEgress",
        files1: [
          { sourcePath: "netapp/folder1/file1.pdf" },
          { sourcePath: "netapp/folder1/folder2/file2.pdf" },
        ],
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () => mockResponse,
      });

      const payload = {
        caseId: 12,
        transferDirection: "EgressToNetApp" as const,
        transferType: "Copy" as const,
        sourcePaths: [
          {
            fileId: "1",
            path: "abc/def",
            isFolder: true,
          },
        ],
        sourceRootFolderPath: "abc/",
        destinationPath: "netapp/",
      };

      await expect(indexingFileTransfer(payload)).rejects.toBeInstanceOf(
        ApiError,
      );
      await expect(indexingFileTransfer(payload)).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/filetransfer/files: response schema validation failed; status - OK (200)",
      );
    });

    it("indexingFileTransfer - should not call fetch and throw Error and console.warn when request schema validation fails ", async () => {
      const mockResponse = {
        caseId: 12,
        isInvalid: false,
        destinationPath: "abc/",
        validationErrors: [],
        sourceRootFolderPath: "netapp/",
        transferDirection: "NetAppToEgress",
        files: [
          { sourcePath: "netapp/folder1/file1.pdf" },
          { sourcePath: "netapp/folder1/folder2/file2.pdf" },
        ],
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        json: () => mockResponse,
      });

      const payload = {
        caseId1: 12,
        transferDirection: "EgressToNetApp" as const,
        transferType: "Copy" as const,
        sourcePaths: [
          {
            fileId: "1",
            path: "abc/def",
            isFolder: true,
          },
        ],
        sourceRootFolderPath: "abc/",
        destinationPath: "netapp/",
      };
      const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});

      await expect(indexingFileTransfer(payload as any)).rejects.toThrow(
        new Error(`Invalid indexing file transfer request payload`),
      );
      expect(fetch).not.toHaveBeenCalled();
      expect(warnSpy).toHaveBeenCalledOnce();
      expect(warnSpy).toHaveBeenCalledWith(
        expect.stringMatching(
          /^Invalid indexing file transfer request payload:/,
        ),
      );

      warnSpy.mockRestore();
    });
  });
  describe("initiateFileTransfer", () => {
    it("initiateFileTransfer - should successfully call the post request and return the response", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: () => ({
          id: "transfer-id-egress-to-netapp",
        }),
      });

      const payload = {
        isRetry: false,
        caseId: 12,
        workspaceId: "thuderstruck",
        transferType: "Copy" as const,
        transferDirection: "EgressToNetApp" as const,
        sourcePaths: [],
        destinationPath: "netapp/",
        sourceRootFolderPath: "egress/",
      };
      const result = await initiateFileTransfer(payload);
      expect(result).toEqual({ id: "transfer-id-egress-to-netapp" });
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

    it("initiateFileTransfer - should throw an ApiError when post request fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });
      const payload = {
        isRetry: false,
        caseId: 12,
        workspaceId: "thunderstruck",
        transferType: "Copy" as const,
        transferDirection: "EgressToNetApp" as const,
        sourcePaths: [],
        destinationPath: "netapp/",
        sourceRootFolderPath: "egress/",
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
    it("initiateFileTransfer -  response schema validation failed", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () => ({
          id1: "transfer-id-egress-to-netapp",
        }),
      });

      const payload = {
        isRetry: false,
        caseId: 12,
        workspaceId: "thuderstruck",
        transferType: "Copy" as const,
        transferDirection: "EgressToNetApp" as const,
        sourcePaths: [],
        destinationPath: "netapp/",
        sourceRootFolderPath: "egress/",
      };

      await expect(initiateFileTransfer(payload)).rejects.toBeInstanceOf(
        ApiError,
      );
      await expect(initiateFileTransfer(payload)).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/filetransfer/initiate: response schema validation failed; status - OK (200)",
      );
    });
    it("initiateFileTransfer - should not call fetch and throw Error and console.warn when request schema validation fails ", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () => ({
          id: "transfer-id-egress-to-netapp",
        }),
      });
      const payload = {
        isRetry: false,
        caseId1: 12,
        workspaceId: "thuderstruck",
        transferType: "Copy" as const,
        transferDirection: "EgressToNetApp" as const,
        sourcePaths: [],
        destinationPath: "netapp/",
        sourceRootFolderPath: "egress/",
      };

      const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});
      await expect(initiateFileTransfer(payload as any)).rejects.toThrow(
        new Error(`Invalid initiate file transfer request payload`),
      );
      expect(fetch).not.toHaveBeenCalled();

      expect(warnSpy).toHaveBeenCalledOnce();
      expect(warnSpy).toHaveBeenCalledWith(
        expect.stringMatching(
          /^Invalid initiate file transfer request payload:/,
        ),
      );

      warnSpy.mockRestore();
    });
  });
  describe("getTransferStatus", () => {
    it("getTransferStatus - should return transferStatus when fetch is successful", async () => {
      const mockData = {
        status: "Completed",
        transferType: "Copy",
        direction: "EgressToNetApp",
        completedAt: null,
        failedItems: [],
        userName: "dev_user@example.org",
        totalFiles: 30,
        processedFiles: 30,
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

    it("getTransferStatus - should throw an ApiError when fetch fails", async () => {
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

    it("getTransferStatus - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(getTransferStatus("transfer_id_1")).rejects.toBeInstanceOf(
        ApiError,
      );
      await expect(getTransferStatus("transfer_id_1")).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/filetransfer/transfer_id_1/status: SyntaxError: Unexpected token < in JSON; status - OK (200)",
      );
    });
    it("getTransferStatus -  response schema validation failed", async () => {
      const mockData = {
        status1: "Completed",
        transferType: "Copy",
        direction: "EgressToNetApp",
        completedAt: null,
        failedItems: [],
        userName: "dev_user@example.org",
        totalFiles: 30,
        processedFiles: 30,
      };

      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(getTransferStatus("transfer_id_1")).rejects.toBeInstanceOf(
        ApiError,
      );
      await expect(getTransferStatus("transfer_id_1")).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/filetransfer/transfer_id_1/status: response schema validation failed; status - OK (200)",
      );
    });
  });
  describe("handleFileTransferClear", () => {
    it("handleFileTransferClear -should handle when post request is successful", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
      });

      await handleFileTransferClear("mock_transfer_Id");
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/mock_transfer_Id/clear`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });

    it("handleFileTransferClear - should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(handleFileTransferClear("mock_transfer_Id")).rejects.toThrow(
        new ApiError(
          `clear file transfer api failed`,
          "gateway_url/api/v1/filetransfer/mock_transfer_Id/clear",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );

      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/filetransfer/mock_transfer_Id/clear`,
        expect.objectContaining({
          method: "POST",
          credentials: "include",
          headers: {
            Authorization: "Bearer access_token",
            "Correlation-Id": "id_123",
          },
        }),
      );
    });
  });
  describe("getActivityLog", () => {
    it("getActivityLog - should return activityLog when fetch is successful", async () => {
      const mockData = {
        data: [
          {
            id: "5",
            actionType: "TRANSFER_INITIATED",
            timestamp: "2024-01-20T12:46:10.865517Z",
            userName: "dwight_schrute@cps.gov.uk",
            caseId: 20,
            resourceName: null,
            description: "Document/folders copying from egress to shared drive",
            details: {
              transferId: "transfer-1",
              totalFiles: 2,
              errorFileCount: 0,
              transferedFileCount: 0,
              sourcePath: "egress",
              destinationPath: "netapp/folder2",
              transferType: "Copy",
              files: [],
              errors: [],
              deletionErrors: [],
            },
          },
        ],
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        json: async () => mockData,
      });

      const result = await getActivityLog("test_case_id");
      expect(result).toEqual(mockData);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/activity/logs?case-id=test_case_id`,
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

    it("getActivityLog - should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(getActivityLog("test_case_id")).rejects.toThrow(
        new ApiError(
          `Getting case activity log failed`,
          "gateway_url/api/v1/activity/logs?case-id=test_case_id",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/activity/logs?case-id=test_case_id`,
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

    it("getActivityLog - invalid json response throws error", async () => {
      (globalThis.fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: () =>
          Promise.reject(new SyntaxError("Unexpected token < in JSON")),
      });

      await expect(getActivityLog("test_case_id")).rejects.toBeInstanceOf(
        ApiError,
      );
      await expect(getActivityLog("test_case_id")).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/activity/logs?case-id=test_case_id: SyntaxError: Unexpected token < in JSON; status - OK (200)",
      );
    });
    it("getTransferStatus -  response schema validation failed", async () => {
      const mockData = {
        data: [
          {
            id: "5",
            actionType: "TRANSFER_INITIATED",
            timestamp: "2024-01-20T12:46:10.865517Z",
            userName: "dwight_schrute@cps.gov.uk",
            caseId: "case_1",
            description: "Document/folders copying from egress to shared drive",
            details: {
              transferId1: "transfer-1",
              totalFiles: 2,
              errorFileCount: 0,
              transferedFileCount: 0,
              sourcePath: "egress",
              destinationPath: "netapp/folder2",
              transferType: "Copy",
              files: [],
              errors: [],
              deletionErrors: [],
            },
          },
        ],
      };
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        status: 200,
        statusText: "OK",
        json: async () => mockData,
      });

      await expect(getActivityLog("test_case_id")).rejects.toBeInstanceOf(
        ApiError,
      );
      await expect(getActivityLog("test_case_id")).rejects.toThrow(
        "An error occurred contacting the server at gateway_url/api/v1/activity/logs?case-id=test_case_id: response schema validation failed; status - OK (200)",
      );
    });
  });

  describe("downloadActivityLog", () => {
    it("downloadActivityLog - should succesfully make the fetch call and return the response", async () => {
      const mockBlob = new Blob(["hello"], { type: "text/csv" });
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: true,
        blob: async () => mockBlob,
      });

      const result = await downloadActivityLog("test_activity_id");
      expect(result.ok).toEqual(true);
      expect(await result.blob()).toEqual(mockBlob);
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/activity/test_activity_id/logs/download`,
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

    it("downloadActivityLog - should throw an ApiError when fetch fails", async () => {
      (v4 as any).mockReturnValue("id_123");
      (getAccessToken as any).mockResolvedValue("access_token");
      (fetch as any).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
      });

      await expect(downloadActivityLog("test_activity_id")).rejects.toThrow(
        new ApiError(
          `Downloading activity log failed`,
          "gateway_url/api/v1/activity/test_activity_id/logs/download",
          {
            status: 500,
            statusText: "Internal Server Error",
          },
        ),
      );
      expect(fetch).toHaveBeenCalledWith(
        `gateway_url/api/v1/activity/test_activity_id/logs/download`,
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
