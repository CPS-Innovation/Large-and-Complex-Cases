import { renderHook, waitFor } from "@testing-library/react";
import { useApi } from "./useApi";

describe("useApi", () => {
  it("can initiate a call, set status to loading, then return a successful result", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = vi.fn(
      () => new Promise((resolve) => setTimeout(() => resolve(mockResult), 10)),
    );

    const { result } = renderHook(() => useApi(mockApiCall, ["1"]));

    expect(result.current).toEqual({
      status: "loading",
      refetch: expect.any(Function),
    });
    await waitFor(() => {
      expect(result.current).toEqual({
        status: "succeeded",
        data: mockResult,
        refetch: expect.any(Function),
      });
    });
  });

  it("can initiate a call, set status to loading, then return an error result", async () => {
    const mockError = new Error();
    const mockApiCall = vi.fn(
      () => new Promise((_, reject) => setTimeout(() => reject(mockError))),
    );

    const { result } = renderHook(() => useApi(mockApiCall, ["1"]));

    expect(result.current).toEqual({
      status: "loading",
      refetch: expect.any(Function),
    });
    await waitFor(() => {
      expect(result.current).toEqual({
        status: "failed",
        error: mockError,
        refetch: expect.any(Function),
      });
    });
  });

  it("can initiate a call with multiple parameters", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = vi.fn(
      () => new Promise((resolve) => setTimeout(() => resolve(mockResult), 10)),
    );

    const { result } = renderHook(() => useApi(mockApiCall, ["1", 2, "3"]));

    expect(result.current).toEqual({
      status: "loading",
      refetch: expect.any(Function),
    });
    await waitFor(() => {
      expect(result.current).toEqual({
        status: "succeeded",
        data: mockResult,
        refetch: expect.any(Function),
      });
    });
  });

  it("can not call the api again if parameters do not change", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = vi.fn(
      () => new Promise((resolve) => setTimeout(() => resolve(mockResult), 10)),
    );

    const { rerender } = renderHook(() => useApi(mockApiCall, ["1"]));

    expect(mockApiCall.mock.calls.length).toEqual(1);
    rerender();
    expect(mockApiCall.mock.calls.length).toEqual(1);
  });

  it("can call the api a second time if parameters do change", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = vi.fn(
      () => new Promise((resolve) => setTimeout(() => resolve(mockResult), 10)),
    );

    const { rerender } = renderHook(({ del, p1 }) => useApi(del, [p1]), {
      initialProps: {
        del: mockApiCall,
        p1: "1",
      },
    });

    expect(mockApiCall.mock.calls.length).toEqual(1);

    rerender({
      del: mockApiCall,
      p1: "2",
    });

    expect(mockApiCall.mock.calls.length).toEqual(2);
  });

  it("should make an api call, only if the 3rd parameter of the useAPi hook is not false", async () => {
    const mockApiCall = vi.fn(
      () =>
        new Promise((resolve) => setTimeout(() => resolve("mockResult"), 10)),
    );

    const { result, rerender } = renderHook(
      ({ makeCall }) => useApi(mockApiCall, ["1"], makeCall),
      { initialProps: { makeCall: false } },
    );
    expect(mockApiCall).not.toHaveBeenCalled();
    expect(result.current).toEqual({
      status: "initial",
      refetch: expect.any(Function),
    });

    //just another assertion to specifically prove the reverse, third parameter by default is true
    rerender({
      makeCall: true,
    });
    expect(mockApiCall).toHaveBeenCalledTimes(1);
    expect(result.current).toEqual({
      status: "loading",
      refetch: expect.any(Function),
    });
  });

  it("should make an api call even if it has not made the api call initially, if we call the refetch", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = vi.fn(
      () => new Promise((resolve) => setTimeout(() => resolve(mockResult), 10)),
    );

    const { result, rerender } = renderHook(() =>
      useApi(mockApiCall, ["1"], false),
    );
    expect(mockApiCall).not.toHaveBeenCalled();
    expect(result.current).toEqual({
      status: "initial",
      refetch: expect.any(Function),
    });

    //call api through refetch
    result.current.refetch();
    rerender();
    expect(mockApiCall).toHaveBeenCalledTimes(1);

    expect(result.current).toEqual({
      status: "loading",
      refetch: expect.any(Function),
    });
    await waitFor(() => {
      expect(result.current).toEqual({
        status: "succeeded",
        data: mockResult,
        refetch: expect.any(Function),
      });
    });
  });

  it("should make an api call even if it has made the api call initially, if we call the refetch", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = vi.fn(
      () => new Promise((resolve) => setTimeout(() => resolve(mockResult), 10)),
    );

    const { result, rerender } = renderHook(() =>
      useApi(mockApiCall, ["1"], true),
    );
    expect(mockApiCall).toHaveBeenCalledTimes(1);
    expect(result.current).toEqual({
      status: "loading",
      refetch: expect.any(Function),
    });
    await waitFor(() => {
      expect(result.current).toEqual({
        status: "succeeded",
        data: mockResult,
        refetch: expect.any(Function),
      });
    });

    //call api through refetch
    result.current.refetch();
    rerender();
    expect(mockApiCall).toHaveBeenCalledTimes(2);

    expect(result.current).toEqual({
      status: "loading",
      refetch: expect.any(Function),
    });
    await waitFor(() => {
      expect(result.current).toEqual({
        status: "succeeded",
        data: mockResult,
        refetch: expect.any(Function),
      });
    });
  });
});
