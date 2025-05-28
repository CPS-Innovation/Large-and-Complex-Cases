/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState, useEffect, useCallback } from "react";
import { ApiError } from "../errors/ApiError";

type ApiStatus = "initial" | "loading" | "succeeded" | "failed";

interface UseApiState<T> {
  status: ApiStatus;
  data?: T;
  error?: ApiError;
}

export interface UseApiResult<T> extends UseApiState<T> {
  refetch: () => void;
}

export const useApi = <T>(
  apiFunction: (...args: any[]) => Promise<T>,
  params: any[] = [],
  makeCall = true,
): UseApiResult<T> => {
  const [result, setResult] = useState<UseApiState<T>>({ status: "initial" });

  const fetchData = useCallback(() => {
    setResult({ status: "loading" });

    apiFunction(...params)
      .then((data) => {
        setResult({ status: "succeeded", data });
      })
      .catch((error: ApiError) => {
        setResult({ status: "failed", error });
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [apiFunction, JSON.stringify(params)]);

  useEffect(() => {
    if (makeCall) {
      fetchData();
    }
  }, [fetchData, makeCall]);

  return { ...result, refetch: fetchData };
};
