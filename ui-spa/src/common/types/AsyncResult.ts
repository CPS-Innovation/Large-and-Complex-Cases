export type AsyncResult<T> =
  | {
      status: "loading";
    }
  | {
      status: "succeeded";
      data: T;
    }
  | {
      status: "failed";
      error: string;
    };
