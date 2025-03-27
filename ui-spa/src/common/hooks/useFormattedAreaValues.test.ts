import { renderHook, waitFor } from "@testing-library/react";
import { useFormattedAreaValues } from "./useFormattedAreaValues";
import { vi, Mock } from "vitest";
import { useMainStateContext } from "../../providers/MainStateProvider";
import { useAsyncActionHandlers } from "../hooks/useAsyncActionHandlers";

vi.mock("../../providers/MainStateProvider", () => {
  return { useMainStateContext: vi.fn() };
});
vi.mock("../hooks/useAsyncActionHandlers", () => {
  return {
    useAsyncActionHandlers: vi.fn(),
  };
});

describe("useFormattedAreaValues", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });
  it("should return value correctly when the caseDivisionsOrAreas status not equal to `succeeded`", async () => {
    const mockState = {
      caseDivisionsOrAreas: {
        status: "loading",
        data: {
          allAreas: [
            { id: 1, description: "allAreas_1" },
            { id: 2, description: "allAreas_2" },
          ],

          userAreas: [
            { id: 3, description: "allAreas_3" },
            { id: 4, description: "allAreas_4" },
          ],

          homeArea: { id: 3, description: "allAreas_3" },
        },
      },
    };
    const handleGetCaseDivisionsOrAreasMock = vi.fn();
    (useAsyncActionHandlers as Mock).mockImplementation(() => ({
      handleGetCaseDivisionsOrAreas: handleGetCaseDivisionsOrAreasMock,
    }));
    (useMainStateContext as Mock).mockReturnValue({
      state: mockState,
      dispatch: () => {},
    });
    const { result, rerender } = renderHook(() => useFormattedAreaValues());
    await waitFor(() => {
      expect(handleGetCaseDivisionsOrAreasMock).toHaveBeenCalledTimes(1);
    });
    expect(result.current).toStrictEqual({
      defaultValue: undefined,
      options: [],
    });

    const newMockState = {
      caseDivisionsOrAreas: {
        status: "succeeded",
        data: {
          allAreas: [
            { id: 1, description: "allAreas_1" },
            { id: 2, description: "allAreas_2" },
          ],

          userAreas: [
            { id: 3, description: "allAreas_3" },
            { id: 4, description: "allAreas_4" },
          ],

          homeArea: { id: 3, description: "allAreas_3" },
        },
      },
    };
    (useMainStateContext as Mock).mockReturnValue({
      state: newMockState,
      dispatch: () => {},
    });
    rerender();
    const expectedResult = {
      defaultValue: 3,
      options: [
        {
          children: "-- Please select --",
          disabled: true,
          value: "",
        },
        {
          children: "Your units/areas",
          disabled: true,
          value: "",
        },
        {
          children: "allAreas_3",
          value: 3,
        },
        {
          children: "allAreas_4",
          value: 4,
        },
        {
          children: "All areas",
          disabled: true,
          value: "",
        },
        {
          children: "allAreas_1",
          value: 1,
        },
        {
          children: "allAreas_2",
          value: 2,
        },
      ],
    };
    expect(handleGetCaseDivisionsOrAreasMock).toHaveBeenCalledTimes(1);
    expect(result.current).toStrictEqual(expectedResult);
  });
  it("should return value correctly when the caseDivisionsOrAreas status is equal to `succeeded`", () => {
    const handleGetCaseDivisionsOrAreasMock = vi.fn();
    (useAsyncActionHandlers as Mock).mockImplementation(() => ({
      handleGetCaseDivisionsOrAreas: handleGetCaseDivisionsOrAreasMock,
    }));
    const mockState = {
      caseDivisionsOrAreas: {
        status: "succeeded",
        data: {
          allAreas: [
            { id: 1, description: "allAreas_1" },
            { id: 2, description: "allAreas_2" },
          ],

          userAreas: [
            { id: 3, description: "allAreas_3" },
            { id: 4, description: "allAreas_4" },
          ],

          homeArea: { id: 3, description: "allAreas_3" },
        },
      },
    };
    (useMainStateContext as Mock).mockReturnValue({
      state: mockState,
      dispatch: () => {},
    });
    const { result } = renderHook(() => useFormattedAreaValues());
    expect(handleGetCaseDivisionsOrAreasMock).toHaveBeenCalledTimes(0);

    const expectedResult = {
      defaultValue: 3,
      options: [
        {
          children: "-- Please select --",
          disabled: true,
          value: "",
        },
        {
          children: "Your units/areas",
          disabled: true,
          value: "",
        },
        {
          children: "allAreas_3",
          value: 3,
        },
        {
          children: "allAreas_4",
          value: 4,
        },
        {
          children: "All areas",
          disabled: true,
          value: "",
        },
        {
          children: "allAreas_1",
          value: 1,
        },
        {
          children: "allAreas_2",
          value: 2,
        },
      ],
    };

    expect(result.current).toStrictEqual(expectedResult);
  });
});
