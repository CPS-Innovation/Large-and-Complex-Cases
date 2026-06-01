import { renderHook, waitFor } from "@testing-library/react";
import { useFormattedAreaValues } from "./useFormattedAreaValues";
import { vi, Mock } from "vitest";
import { useMainStateContext } from "../../providers/MainStateProvider";
vi.mock("../../providers/MainStateProvider", () => {
  return { useMainStateContext: vi.fn() };
});

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
      apiData: {
        caseDivisionsOrAreas: null,
      },
    };
    (useMainStateContext as Mock).mockReturnValue({
      state: mockState,
      dispatch: () => {},
    });
    const { result } = renderHook(() => useFormattedAreaValues());
    expect(result.current).toStrictEqual({
      defaultValue: undefined,
      options: [],
    });
  });
  it("should return formatted area values correctly when caseDivisionsOrAreas is available", async () => {
    const mockState = {
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

    (useMainStateContext as Mock).mockReturnValue({
      state: mockState,
      dispatch: () => {},
    });
    const { result } = renderHook(() => useFormattedAreaValues());

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
