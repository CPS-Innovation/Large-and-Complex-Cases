import { ApiError } from "./ApiError";
describe("ApiError", () => {
  it("Should create an ApiError with correct properties", () => {
    const error = new ApiError(
      "test api error",
      "/api/test",
      {
        status: 500,
        statusText: "internal server error",
      },
      {
        customProperties: { retry: "true" },
        customMessage: "test api error",
        correlationId: "test-corr-id-123",
      },
    );

    expect(error).toEqual(
      expect.objectContaining({
        message:
          "API Error: /api/test returned 500 internal server error - test api error",
        name: "API_ERROR",
        path: "/api/test",
        code: 500,
        customProperties: {
          retry: "true",
        },
        customMessage: "test api error",
        correlationId: "test-corr-id-123",
      }),
    );
  });
});

it("should throw an ApiError", () => {
  const thrower = () => {
    throw new ApiError("api error", "/some/path", {
      status: 500,
      statusText: "Internal Server Error",
    });
  };

  expect(thrower).toThrowError(ApiError);
  expect(thrower).toThrowError(
    "API Error: /some/path returned 500 Internal Server Error - api error",
  );
});
