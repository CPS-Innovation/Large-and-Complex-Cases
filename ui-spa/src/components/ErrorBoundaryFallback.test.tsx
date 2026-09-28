import { render, screen, waitFor } from "@testing-library/react";
import { type Mock, describe, it, expect, vi, beforeEach } from "vitest";
import { telemetryService } from "../TelemetryLogger";
import { ErrorBoundaryFallback } from "./ErrorBoundaryFallback";
import { ApiError } from "../common/errors/ApiError";
import { MemoryRouter } from "react-router";
vi.mock("../TelemetryLogger", () => ({
  telemetryService: {
    trackException: vi.fn(),
  },
}));

beforeEach(() => {
  vi.clearAllMocks();
});

describe("ErrorBoundaryFallback", () => {
  it("logs ApiError with correlationId and renders correlationId", async () => {
    const apiErr = new ApiError(
      "api error",
      "/path",
      {
        status: 500,
        statusText: "Server Error",
      },
      {
        correlationId: "corr-123",
      },
    );

    render(
      <MemoryRouter>
        <ErrorBoundaryFallback error={apiErr} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(telemetryService.trackException).toHaveBeenCalledTimes(1);
    });

    expect((telemetryService.trackException as Mock).mock.calls[0]).toEqual([
      apiErr,
      [{ correlationId: "corr-123" }],
    ]);

    expect(screen.getByTestId("txt-error-page-heading")).toHaveTextContent(
      "Sorry, there is a problem with the service",
    );
    expect(
      screen.getByText(
        "Contact the product team and give them the error code.",
      ),
    ).toBeInTheDocument();
    expect(screen.getByText("Error code: corr-123")).toBeInTheDocument();
  });

  // it("logs generic Error once and renders error message", async () => {
  //   const err = new Error("boom");

  //   render(
  //     <MemoryRouter>
  //       <ErrorBoundaryFallback error={err} />
  //     </MemoryRouter>,
  //   );

  //   await waitFor(() => {
  //     expect(telemetryService.trackException).toHaveBeenCalledTimes(1);
  //   });

  //   expect((telemetryService.trackException as Mock).mock.calls[0]).toEqual([
  //     err,
  //   ]);

  //   expect(screen.getByTestId("txt-error-page-heading")).toHaveTextContent(
  //     "Sorry, there is a problem with the service",
  //   );
  //   expect(
  //     screen.getByText(
  //       "Contact the product team and give them the error code.",
  //     ),
  //   ).toBeInTheDocument();
  //   expect(screen.getByText("Error code: boom")).toBeInTheDocument();
  // });
});
