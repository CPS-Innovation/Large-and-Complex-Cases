import { renderHook } from "@testing-library/react";
import { useFormattedAreaValues } from "./useFormattedAreaValues";
import { vi, Mock } from "vitest";
import { useMainStateContext } from "../../providers/MainStateProvider";
import { useGetCaseDivisionOrAreas } from "../../common/hooks/useGetAppLevelLookups";

vi.mock("../../providers/MainStateProvider", () => {
  return { useMainStateContext: vi.fn() };
});
vi.mock("../../common/hooks/useGetAppLevelLookups", () => {
  return { useGetCaseDivisionOrAreas: vi.fn().mockImplementation(() => {}) };
});

describe("useFormattedAreaValues", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });
  it("should return value correctly when the caseDivisionsOrAreas status not equal to `succeeded`", () => {
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
    (useMainStateContext as Mock).mockReturnValue({
      state: mockState,
      dispatch: () => {},
    });
    const { result, rerender } = renderHook(() => useFormattedAreaValues());
    expect(useGetCaseDivisionOrAreas).toHaveBeenCalledTimes(1);
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
    expect(useGetCaseDivisionOrAreas).toHaveBeenCalledTimes(2);
    expect(result.current).toStrictEqual(expectedResult);
  });
  it("should return value correctly when the caseDivisionsOrAreas status is equal to `succeeded`", () => {
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
    expect(useGetCaseDivisionOrAreas).toHaveBeenCalledTimes(1);

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
