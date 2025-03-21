import { renderHook } from "@testing-library/react";
import { vi, Mock } from "vitest";
import { useNavigate, useSearchParams } from "react-router-dom";
import useSearchNavigation from "./useSearchNavigation";

vi.mock("react-router-dom", () => {
  return { useNavigate: vi.fn(), useSearchParams: vi.fn() };
});

describe("useSearchNavigation", () => {
  it("Should return valid search param values based on useSearchParam hook", () => {
    (useSearchParams as Mock).mockReturnValue([
      new URLSearchParams(
        "urn=abc&operation-name=op1&defendant-name=def1&area=123",
      ),
      vi.fn(),
    ]);
    const { result } = renderHook(() => useSearchNavigation);

    expect(result?.current().searchParams).toEqual({
      area: "123",
      "defendant-name": "def1",
      "operation-name": "op1",
      urn: "abc",
    });
    expect(result?.current().queryString).toEqual(
      "urn=abc&operation-name=op1&defendant-name=def1&area=123",
    );
  });
  it("Should ignore any params other than the one defined in `SearchParamsType`", () => {
    (useSearchParams as Mock).mockReturnValue([
      new URLSearchParams(
        "urn1=&operation-name1=&defendant-name1=&area1=false",
      ),
      vi.fn(),
    ]);
    const { result } = renderHook(() => useSearchNavigation);

    expect(result?.current().searchParams).toEqual({});
    expect(result?.current().queryString).toEqual("");

    (useSearchParams as Mock).mockReturnValue([
      new URLSearchParams(
        "urn1=&operation-name1=&defendant-name1=&area1=false&area=123",
      ),
      vi.fn(),
    ]);
    const { result: newResult } = renderHook(() => useSearchNavigation);

    expect(newResult?.current().searchParams).toEqual({ area: "123" });
    expect(newResult?.current().queryString).toEqual("area=123");
  });
  it("Should update the searchParam by calling the updateSearchParams", () => {
    const setParamMock = vi.fn();
    (useSearchParams as Mock).mockReturnValue([
      new URLSearchParams(""),
      setParamMock,
    ]);
    const { result } = renderHook(() => useSearchNavigation);
    const newParams = {
      urn: "abc",
      "operation-name": "op-name",
      "defendant-name": "def-name",
      area: "123",
    };
    result?.current().updateSearchParams(newParams);
    expect(setParamMock).toHaveBeenCalledOnce();
    expect(setParamMock).toHaveBeenCalledWith(newParams);
  });

  it("Should call navigate with correct params when calling navigateWithParams ", () => {
    const navigateMock = vi.fn();
    (useNavigate as Mock).mockReturnValue(navigateMock);
    const { result } = renderHook(() => useSearchNavigation);
    const newParams = {
      urn: "abc",
      "operation-name": "op-name",
      "defendant-name": "def-name",
      area: "123",
    };
    result?.current().navigateWithParams(newParams);
    expect(navigateMock).toHaveBeenCalledOnce();
    expect(navigateMock).toHaveBeenCalledWith(
      `/search-results?urn=abc&operation-name=op-name&defendant-name=def-name&area=123`,
    );
  });
});
