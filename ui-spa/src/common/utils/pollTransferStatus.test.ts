import { vi, Mock } from "vitest";
import { pollTransferStatus } from "./pollTransferStatus";
import { getTransferStatus } from "../../apis/gateway-api";
import { ApiError } from "../../common/errors/ApiError";

vi.mock("../../apis/gateway-api", () => ({
  getTransferStatus: vi.fn(),
}));

const mockStatus = (status: string) => ({
  data: { status },
  etag: null,
});

describe("pollTransferStatus", async () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.resetAllMocks();
  });
  afterEach(() => {
    // Several tests start a poll loop with shouldStopPolling = () => false,
    // which never terminates on its own. Clear any pending timers and restore
    // real timers so a leaked loop cannot bleed into the next test.
    vi.clearAllTimers();
    vi.useRealTimers();
  });
  it("Should poll for the getTransferStatus endpoint until shouldStopPolling return true", async () => {
    vi.useFakeTimers();
    let stopAfter = 3;
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => --stopAfter <= 0);
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("InProgress"));

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
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("Initiated"));

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
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("Initiated"));
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(2);
    expect(handleResponse).to.toHaveBeenCalledTimes(2);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(2);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("InProgress"));
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(3);
    expect(handleResponse).to.toHaveBeenCalledTimes(3);
    expect(handleResponse).to.toHaveBeenCalledWith({ status: "InProgress" });
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("Completed"));
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
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("Initiated"));
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(2);
    expect(handleResponse).to.toHaveBeenCalledTimes(1);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(2);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("Completed"));
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(3);
    expect(handleResponse).to.toHaveBeenCalledTimes(2);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);
    expect(handleError).to.toHaveBeenCalledTimes(0);
    expect(handleError).to.toHaveBeenCalledTimes(0);
  });

  it("Should not drop below the size-based interval when consecutive 304s trigger stepped backoff on a large transfer", async () => {
    vi.useFakeTimers();
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => false);
    // 500 files: base interval = min(100 + 500*10, 5000) = 5000ms (the cap)
    (getTransferStatus as Mock).mockResolvedValueOnce({
      data: { status: "InProgress", totalFiles: 500 },
      etag: '"etag-1"',
    });
    // Subsequent calls are 304 Not Modified
    (getTransferStatus as Mock).mockResolvedValue({
      data: null,
      etag: '"etag-1"',
    });

    pollTransferStatus(
      "id-1",
      shouldStopPolling,
      handleResponse,
      handleError,
      100,
    );

    // Initial 200 response sets the adaptive base to the 5s cap
    await vi.advanceTimersByTimeAsync(0);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(1);

    // First 304 (noChangeCount = 1): interval stays at base 5s
    await vi.advanceTimersByTimeAsync(5000);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(2);

    // Second 304 (noChangeCount = 2): stepped value is 3s but base is the floor
    await vi.advanceTimersByTimeAsync(5000);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);

    // The interval must not drop to 3s
    await vi.advanceTimersByTimeAsync(3000);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);

    // It only fires once the full 5s base interval has elapsed
    await vi.advanceTimersByTimeAsync(2000);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(4);
    expect(handleError).to.toHaveBeenCalledTimes(0);
  });

  it("Should reset the stepped backoff to the size-based interval on a 200 response", async () => {
    vi.useFakeTimers();
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => false);
    // 10 files: base interval = 100 + 10*10 = 200ms
    (getTransferStatus as Mock).mockResolvedValueOnce({
      data: { status: "InProgress", totalFiles: 10 },
      etag: '"etag-1"',
    });
    (getTransferStatus as Mock).mockResolvedValueOnce({
      data: null,
      etag: '"etag-1"',
    });
    (getTransferStatus as Mock).mockResolvedValueOnce({
      data: null,
      etag: '"etag-1"',
    });
    (getTransferStatus as Mock).mockResolvedValue({
      data: { status: "InProgress", totalFiles: 10 },
      etag: '"etag-2"',
    });

    pollTransferStatus(
      "id-1",
      shouldStopPolling,
      handleResponse,
      handleError,
      100,
    );

    await vi.advanceTimersByTimeAsync(0);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(1);

    // First 304 (noChangeCount = 1): interval stays at 200ms base
    await vi.advanceTimersByTimeAsync(200);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(2);

    // Second 304 (noChangeCount = 2): backoff steps up to 3s
    await vi.advanceTimersByTimeAsync(200);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);
    await vi.advanceTimersByTimeAsync(2999);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(3);
    await vi.advanceTimersByTimeAsync(1);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(4);

    // 200 response resets the backoff: next poll is back at the 200ms base
    await vi.advanceTimersByTimeAsync(200);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(5);
    expect(handleError).to.toHaveBeenCalledTimes(0);
  });

  it("Should call handleError and stop polling after a bounded run of consecutive 404s", async () => {
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

    // 12 consecutive 404s at the 100ms base interval. The 12th poll fires at
    // t = 11 * 100 = 1100ms, so advancing 1200ms covers the whole grace window.
    await vi.advanceTimersByTimeAsync(1200);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(12);
    expect(handleResponse).to.toHaveBeenCalledTimes(0);
    expect(handleError).to.toHaveBeenCalledTimes(1);

    const errorArg = (handleError as Mock).mock.calls[0][0];
    expect(errorArg).toBeInstanceOf(ApiError);
    expect((errorArg as ApiError).code).toBe(404);

    // The loop has given up: no further polling and no further errors.
    await vi.advanceTimersByTimeAsync(5000);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(12);
    expect(handleError).to.toHaveBeenCalledTimes(1);
  });

  it("Should reset the 404 grace window when the entity appears, so an interrupted run of 404s does not give up", async () => {
    vi.useFakeTimers();
    const handleResponse = vi.fn();
    const handleError = vi.fn();
    const shouldStopPolling = vi.fn(() => false);
    const notFound = new ApiError("not found", "abc/", {
      status: 404,
      statusText: "notFound",
    });

    // 11 404s (one short of the cap), a 200 that resets the window, then
    // another 11 404s. Without a reset this would exceed the cap, but the 200
    // in the middle keeps the loop alive.
    for (let i = 0; i < 11; i++) {
      (getTransferStatus as Mock).mockRejectedValueOnce(notFound);
    }
    (getTransferStatus as Mock).mockResolvedValueOnce(mockStatus("InProgress"));
    for (let i = 0; i < 11; i++) {
      (getTransferStatus as Mock).mockRejectedValueOnce(notFound);
    }
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("InProgress"));

    pollTransferStatus(
      "id-1",
      shouldStopPolling,
      handleResponse,
      handleError,
      100,
    );

    await vi.advanceTimersByTimeAsync(100 * 30);
    expect(handleResponse).to.toHaveBeenCalled();
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
    (getTransferStatus as Mock).mockResolvedValue(mockStatus("Initiated"));
    await vi.advanceTimersByTimeAsync(99);
    expect(shouldStopPolling).to.toHaveBeenCalledTimes(1);
    expect(handleResponse).to.toHaveBeenCalledTimes(0);
    expect(getTransferStatus).to.toHaveBeenCalledTimes(1);
    expect(handleError).to.toHaveBeenCalledTimes(1);
  });
});
