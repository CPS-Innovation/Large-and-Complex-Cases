import { renderHook } from "@testing-library/react";
import { useFormattedAreaValues } from "./useFormattedAreaValues";
import { vi } from "vitest";
import { MainStateContext } from "../../providers/MainStateProvider";
import { createElement } from "react";

vi.mock("react-router", () => {
  return {
    useLocation: vi.fn().mockImplementation(() => ({
      pathname: "",
    })),
  };
});

describe("useFormattedAreaValues", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("Should return correct values when caseDivisionsOrAreas is not available", () => {
    const mockState = {
      appData: { featureFlags: {} } as any,
      formData: {} as any,
      apiData: {
        caseDivisionsOrAreas: null,
      },
    };
    const wrapper = ({ children }: any) =>
      createElement(
        MainStateContext.Provider,
        { value: { state: mockState, dispatch: () => {} } },
        children,
      );

    const { result } = renderHook(() => useFormattedAreaValues(), { wrapper });
    expect(result.current).toStrictEqual({
      defaultValue: undefined,
      options: [],
    });
  });
  it("should return formatted area values correctly when caseDivisionsOrAreas is available", async () => {
    const mockState = {
      appData: { featureFlags: {} } as any,
      formData: {} as any,
      apiData: {
        caseDivisionsOrAreas: {
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
    const wrapper = ({ children }: any) =>
      createElement(
        MainStateContext.Provider,
        { value: { state: mockState, dispatch: () => {} } },
        children,
      );

    const { result } = renderHook(() => useFormattedAreaValues(), { wrapper });

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
