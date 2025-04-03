import { useState, useEffect, useCallback } from "react";

type ApiStatus = "initial" | "loading" | "succeeded" | "failed";

interface UseApiState<T> {
  status: ApiStatus;
  data?: T;
  error?: any;
}

interface UseApiResult<T> extends UseApiState<T> {
  refetch: () => void;
}

export const useApiNew = <T>(
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
      .catch((error) => {
        setResult({ status: "failed", error });
      });
  }, [apiFunction, JSON.stringify(params)]);

  useEffect(() => {
    if (makeCall) {
      fetchData();
    }
  }, [fetchData, makeCall]);

  return { ...result, refetch: fetchData };
};
