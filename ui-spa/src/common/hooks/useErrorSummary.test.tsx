import { renderHook } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { useErrorSummary } from "./useErrorSummary";

describe("useErrorSummary", () => {
  it("should return empty errorList when formDataErrors is empty", () => {
    const errorSummaryProperties = vi.fn();
    const { result } = renderHook(() =>
      useErrorSummary({
        formDataErrors: {},
        errorSummaryProperties,
      }),
    );

    expect(result.current.errorList).toEqual([]);
    expect(errorSummaryProperties).not.toHaveBeenCalled();
  });

  it("should return errorList with error properties when formDataErrors has errors", () => {
    const errorSummaryProperties = vi.fn((errorKey) => ({
      children: `Error for ${String(errorKey)}`,
      href: `#${String(errorKey)}`,
      "data-testid": `error-${String(errorKey)}`,
    }));

    const formDataErrors = {
      fieldOne: {
        errorSummaryText: "Field one is required",
        inputErrorText: "This field is required",
      },
      fieldTwo: {
        errorSummaryText: "Field two is invalid",
      },
    };

    const { result } = renderHook(() =>
      useErrorSummary({
        formDataErrors,
        errorSummaryProperties,
      }),
    );

    expect(result.current.errorList).toHaveLength(2);
    expect(result.current.errorList[0]).toEqual({
      reactListKey: "0",
      children: "Error for fieldOne",
      href: "#fieldOne",
      "data-testid": "error-fieldOne",
    });
    expect(result.current.errorList[1]).toEqual({
      reactListKey: "1",
      children: "Error for fieldTwo",
      href: "#fieldTwo",
      "data-testid": "error-fieldTwo",
    });
    expect(errorSummaryProperties).toHaveBeenCalledTimes(2);
  });

  it("should filter out errors with undefined values", () => {
    const errorSummaryProperties = vi.fn((errorKey) => ({
      children: `Error for ${String(errorKey)}`,
      href: `#${String(errorKey)}`,
      "data-testid": `error-${String(errorKey)}`,
    }));

    const formDataErrors = {
      fieldOne: {
        errorSummaryText: "Field one is required",
      },
      fieldTwo: undefined,
      fieldThree: {
        errorSummaryText: "Field three is invalid",
      },
    };

    const { result } = renderHook(() =>
      useErrorSummary({
        formDataErrors,
        errorSummaryProperties,
      }),
    );

    expect(result.current.errorList).toHaveLength(2);
    expect(errorSummaryProperties).toHaveBeenCalledTimes(2);
    expect(errorSummaryProperties).toHaveBeenCalledWith("fieldOne");
    expect(errorSummaryProperties).toHaveBeenCalledWith("fieldThree");
  });

  it("should update errorList when formDataErrors changes", () => {
    const errorSummaryProperties = vi.fn((errorKey) => ({
      children: `Error for ${String(errorKey)}`,
      href: `#${String(errorKey)}`,
      "data-testid": `error-${String(errorKey)}`,
    }));

    const { result, rerender } = renderHook(
      ({ formDataErrors, errorSummaryProperties: props }) =>
        useErrorSummary({
          formDataErrors,
          errorSummaryProperties: props,
        }),
      {
        initialProps: {
          formDataErrors: {
            fieldOne: { errorSummaryText: "Error one" },
          } as Record<string, { errorSummaryText: string }>,
          errorSummaryProperties,
        },
      },
    );

    expect(result.current.errorList).toHaveLength(1);

    rerender({
      formDataErrors: {
        fieldOne: { errorSummaryText: "Error one" },
        fieldTwo: { errorSummaryText: "Error two" },
      } as Record<string, { errorSummaryText: string }>,
      errorSummaryProperties,
    });

    expect(result.current.errorList).toHaveLength(2);
  });
  it("should focus errorSummaryRef element when errors are present", () => {
    const errorSummaryProperties = vi.fn((key) => ({
      children: `Error ${String(key)}`,
      href: `#${String(key)}`,
      "data-testid": `error-${String(key)}`,
    }));

    const { result, rerender } = renderHook(() =>
      useErrorSummary({
        formDataErrors: { fieldOne: { errorSummaryText: "Error one" } },
        errorSummaryProperties,
      }),
    );

    const mockElement = document.createElement("div");
    const focusSpy = vi.spyOn(mockElement, "focus");

    result.current.errorSummaryRef.current = mockElement;
    rerender();

    expect(focusSpy).toHaveBeenCalled();
    focusSpy.mockRestore();
  });

  it("should not focus errorSummaryRef when errorList is empty", () => {
    const errorSummaryProperties = vi.fn();

    const { result } = renderHook(() =>
      useErrorSummary({
        formDataErrors: {},
        errorSummaryProperties,
      }),
    );

    expect(result.current.errorList).toHaveLength(0);
    expect(result.current.errorSummaryRef.current).toBeNull();
  });
});
