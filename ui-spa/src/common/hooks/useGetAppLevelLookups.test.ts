import { renderHook, waitFor } from "@testing-library/react";
import { vi, Mock } from "vitest";
import { getCaseDivisionsOrAreas } from "../../apis/gateway-api";
import { useGetCaseDivisionOrAreas } from "./useGetAppLevelLookups";

vi.mock("../../apis/gateway-api", () => {
  return { getCaseDivisionsOrAreas: vi.fn() };
});

describe("useGetAppLevelLookups", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("should not make the api call, if the caseDivisionsOrAreas status is equal to `succeeded`", () => {
    const caseDivisionsOrAreas = {
      status: "succeeded" as const,
      data: {
        allAreas: [{ id: 1, description: "allAreas_1" }],

        userAreas: [{ id: 3, description: "allAreas_3" }],

        homeArea: { id: 3, description: "allAreas_3" },
      },
    };

    const mockDispatch = vi.fn();
    renderHook(() =>
      useGetCaseDivisionOrAreas(caseDivisionsOrAreas, mockDispatch),
    );
    expect(mockDispatch).toHaveBeenCalledTimes(0);
    expect(getCaseDivisionsOrAreas).toHaveBeenCalledTimes(0);
  });

  it("should make the api call, if the caseDivisionsOrAreas status is not equal to `succeeded`", async () => {
    const caseDivisionsOrAreas = {
      status: "loading" as const,
    };

    const mockDispatch = vi.fn();
    (getCaseDivisionsOrAreas as Mock).mockReturnValue({ result: {} });

    renderHook(() =>
      useGetCaseDivisionOrAreas(caseDivisionsOrAreas, mockDispatch),
    );

    expect(getCaseDivisionsOrAreas).toHaveBeenCalledTimes(1);
    await waitFor(() => {
      expect(mockDispatch).toHaveBeenCalledTimes(2);
      expect(mockDispatch).toHaveBeenNthCalledWith(1, {
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "loading" },
      });
      expect(mockDispatch).toHaveBeenNthCalledWith(2, {
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "succeeded", data: { result: {} } },
      });
    });
  });
});
