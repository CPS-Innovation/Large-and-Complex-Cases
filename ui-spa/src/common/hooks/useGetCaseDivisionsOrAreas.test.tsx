import React from "react";
import { render, waitFor } from "@testing-library/react";
import { renderHook } from "@testing-library/react";
import { vi, describe, it, expect, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { type MainState } from "../../reducers/mainStateReducer";

// mock react-router's useNavigate
const mockNavigate = vi.fn();
vi.mock("react-router", () => ({
  useNavigate: () => mockNavigate,
}));

// mock gateway api
const mockGet = vi.fn();
vi.mock("../../apis/gateway-api", () => ({
  getCaseDivisionsOrAreas: () => mockGet(),
}));

import { MainStateContext } from "../../providers/MainStateProvider";
import { useGetCaseDivisionsOrAreas } from "./useGetCaseDivisionsOrAreas";
import { ApiError } from "../errors/ApiError";

const makeWrapper = (
  providerValue: React.ContextType<typeof MainStateContext>,
) => {
  const Wrapper = ({ children }: { children?: React.ReactNode }) => (
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MainStateContext.Provider value={providerValue}>
        {children}
      </MainStateContext.Provider>
    </QueryClientProvider>
  );
  Wrapper.displayName = "QueryWrapper";
  return Wrapper;
};

describe("useGetCaseDivisionsOrAreas (renderHook)", () => {
  beforeEach(() => {
    mockNavigate.mockReset();
    mockGet.mockReset();
  });

  it("navigates to /unauthorised when API returns 401", async () => {
    mockGet.mockRejectedValue(
      new ApiError("Unauthorized", "/api", {
        status: 401,
        statusText: "Unauthorized",
      }),
    );

    const providerValue = {
      state: { apiData: { caseDivisionsOrAreas: null } } as MainState,
      dispatch: vi.fn(),
    } as React.ContextType<typeof MainStateContext>;

    renderHook(() => useGetCaseDivisionsOrAreas(), {
      wrapper: makeWrapper(providerValue),
    });

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith("/unauthorised", {
        replace: true,
      });
    });
  });

  it("dispatches SET_CASE_DIVISIONS_OR_AREAS when API returns data", async () => {
    const mockData = {
      allAreas: [{ id: 1, description: "A" }],
      userAreas: [{ id: 2, description: "B" }],
      homeArea: { id: 2, description: "B" },
    };
    mockGet.mockResolvedValue(mockData);

    const dispatch = vi.fn();
    const providerValue = {
      state: { apiData: { caseDivisionsOrAreas: null } } as MainState,
      dispatch,
    } as React.ContextType<typeof MainStateContext>;

    renderHook(() => useGetCaseDivisionsOrAreas(), {
      wrapper: makeWrapper(providerValue),
    });

    await waitFor(() => {
      expect(mockGet).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalledWith({
        type: "SET_CASE_DIVISIONS_OR_AREAS",
        payload: { caseDivisionsOrAreas: mockData },
      });
    });
  });

  it("throws when ApiError status is not 401", async () => {
    mockGet.mockRejectedValue(
      new ApiError("Server error", "/api", {
        status: 500,
        statusText: "Internal Server Error",
      }),
    );

    const providerValue = {
      state: { apiData: { caseDivisionsOrAreas: null } } as MainState,
      dispatch: vi.fn(),
    } as React.ContextType<typeof MainStateContext>;

    let captured: unknown = null;

    class ErrorBoundary extends React.Component<
      { children?: React.ReactNode },
      { hasError: boolean }
    > {
      constructor(props: { children?: React.ReactNode }) {
        super(props);
        this.state = { hasError: false };
      }
      componentDidCatch(error: unknown) {
        this.setState({ hasError: true });
        captured = error;
      }
      render(): React.ReactNode {
        return this.state.hasError ? null : (this.props.children ?? null);
      }
    }

    render(
      <QueryClientProvider
        client={
          new QueryClient({ defaultOptions: { queries: { retry: false } } })
        }
      >
        <MainStateContext.Provider value={providerValue}>
          <ErrorBoundary>
            {(() => {
              function C() {
                useGetCaseDivisionsOrAreas();
                return null;
              }
              return <C />;
            })()}
          </ErrorBoundary>
        </MainStateContext.Provider>
      </QueryClientProvider>,
    );

    await waitFor(() => {
      expect(captured).toBeInstanceOf(Error);
      expect((captured as Error).message).toContain("Internal Server Error");
    });
  });
});
