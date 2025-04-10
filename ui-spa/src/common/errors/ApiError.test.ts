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
      { retry: "true" },
      "test api error",
    );

    expect(error).toEqual(
      expect.objectContaining({
        message:
          "An error occurred contacting the server at /api/test: test api error; status - internal server error (500)",
        name: "API_ERROR",
        path: "/api/test",
        code: 500,
        customProperties: {
          retry: "true",
        },
        customMessage: "test api error",
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
    "An error occurred contacting the server at /some/path: api error; status - Internal Server Error (500)",
  );
});
