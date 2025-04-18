export type RawApiResult<T> =
  | {
      status: "initial";
    }
  | {
      status: "loading";
    }
  | {
      error: string;
      status: "failed";
    }
  | {
      status: "succeeded";
      data: T;
    };

export type ApiResult<T> = Exclude<
  RawApiResult<T>,
  {
    status: "initial";
  }
>;
