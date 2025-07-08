import { vi, Mock } from "vitest";
import { pollTransferStatus } from "./pollTransferStatus";
import { getTransferStatus } from "../../apis/gateway-api";
import { ApiError } from "../../common/errors/ApiError";

vi.mock("../../apis/gateway-api", () => ({
  getTransferStatus: vi.fn(),
}));

describe("pollTransferStatus", async () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.resetAllMocks();
  });
  it("Should poll for the getTransferStatus endpoint until shouldStopPolling return true", async () => {
    vi.useFakeTimers();
    let stopAfter = 3;
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => --stopAfter <= 0);
    (getTransferStatus as Mock).mockResolvedValue({
      status: "InProgress",
    });

    pollTransferStatus(
      "id-1",
      shouldStopPolling,
      handleResponse,
      handleError,
      100,
    );
    await vi.advanceTimersByTimeAsync(1000);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(3);
    expect(handleResponse).to.toHaveBeenCalledTimes(2);
    expect(handleResponse).to.toHaveBeenCalledWith({ status: "InProgress" });
    expect(getTransferStatus).to.toHaveBeenCalledTimes(2);
    expect(handleError).to.toHaveBeenCalledTimes(0);
  });

  it("Should poll for the getTransferStatus endpoint until the response status is not equal to 'InProgress'", async () => {
    vi.useFakeTimers();
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => false);
    (getTransferStatus as Mock).mockResolvedValue({
      status: "Initiated",
    });

    pollTransferStatus(
      "id-1",
      shouldStopPolling,
      handleResponse,
      handleError,
      100,
    );

    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(1);
    expect(handleResponse).to.toHaveBeenCalledTimes(1);
    expect(handleResponse).to.toHaveBeenCalledWith({ status: "Initiated" });
    expect(getTransferStatus).to.toHaveBeenCalledTimes(1);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue({
      status: "Initiated",
    });
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(2);
    expect(handleResponse).to.toHaveBeenCalledTimes(2);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(2);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue({
      status: "InProgress",
    });
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(3);
    expect(handleResponse).to.toHaveBeenCalledTimes(3);
    expect(handleResponse).to.toHaveBeenCalledWith({ status: "InProgress" });
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue({
      status: "Completed",
    });
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(4);
    expect(handleResponse).to.toHaveBeenCalledWith({ status: "Completed" });
    expect(handleResponse).to.toHaveBeenCalledTimes(4);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(4);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(4);
    expect(handleResponse).to.toHaveBeenCalledTimes(4);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(4);
    expect(handleError).to.toHaveBeenCalledTimes(0);
  });

  it("Should not call handleError function if there is an ApiError with code 404 and continue the polling", async () => {
    vi.useFakeTimers();
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => false);
    (getTransferStatus as Mock).mockRejectedValue(
      new ApiError("not found", "abc/", {
        status: 404,
        statusText: "notFound",
      }),
    );

    pollTransferStatus(
      "id-1",
      shouldStopPolling,
      handleResponse,
      handleError,
      100,
    );

    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(1);
    expect(handleResponse).to.toHaveBeenCalledTimes(0);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(1);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue({
      status: "Initiated",
    });
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(2);
    expect(handleResponse).to.toHaveBeenCalledTimes(1);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(2);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue({
      status: "Completed",
    });
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(3);
    expect(handleResponse).to.toHaveBeenCalledTimes(2);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    expect(handleError).to.toHaveBeenCalledTimes(0);
  });

  it("Should call handleError function if there is an ApiError code other than 404 and stop the polling", async () => {
    vi.useFakeTimers();
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => false);
    (getTransferStatus as Mock).mockRejectedValue(
      new ApiError("not found", "abc/", {
        status: 500,
        statusText: "notFound",
      }),
    );

    pollTransferStatus(
      "id-1",
      shouldStopPolling,
      handleResponse,
      handleError,
      100,
    );

    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(1);
    expect(handleResponse).to.toHaveBeenCalledTimes(0);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(1);
    expect(handleError).to.toHaveBeenCalledTimes(1);
    expect(handleError).toHaveBeenCalledWith(
      new ApiError("not found", "abc/", {
        status: 500,
        statusText: "notFound",
      }),
    );
    (getTransferStatus as Mock).mockResolvedValue({
      status: "Initiated",
    });
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(1);
    expect(handleResponse).to.toHaveBeenCalledTimes(0);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(1);
    expect(handleError).to.toHaveBeenCalledTimes(1);
  });
});
