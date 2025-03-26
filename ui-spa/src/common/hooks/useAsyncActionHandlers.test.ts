import { renderHook } from "@testing-library/react";
import { useAsyncActionHandlers } from "./useAsyncActionHandlers";
import { vi, Mock } from "vitest";
import { useMainStateContext } from "../../providers/MainStateProvider";

vi.mock("../../providers/MainStateProvider", () => {
  return { useMainStateContext: vi.fn() };
});

describe("useAsyncActionHandlers", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("Should successfully dispatch `GET_CASE_DIVISIONS_OR_AREAS` by calling handleGetCaseDivisionsOrAreas", () => {
    const dispatchMock = vi.fn();
    (useMainStateContext as Mock).mockImplementation(() => ({
      dispatch: dispatchMock,
    }));

    const { result } = renderHook(() => useAsyncActionHandlers());

    expect(useMainStateContext).toHaveBeenCalledTimes(1);
    result.current.handleGetCaseDivisionsOrAreas();
    expect(dispatchMock).toHaveBeenCalledTimes(1);
    expect(dispatchMock).toHaveBeenCalledWith({
      type: "GET_CASE_DIVISIONS_OR_AREAS",
    });
  });
});
