import { useNavigate, useLocation } from "react-router";
import { parse, stringify } from "qs";

export type QueryParamsState<T> = T & {
  setParams: (params: Partial<T>) => void;
  search: string;
};
const path = "/cases";
export const useQueryParamsState = <T>(): QueryParamsState<T> => {
  const { search } = useLocation();
  const navigate = useNavigate();

  const params = parse(search, {
    ignoreQueryPrefix: true,
    comma: true,
  }) as unknown as T;

  console.log("params>>", params);

  const setParams = (params: Partial<T>) => {
    const queryString = stringify(params, {
      addQueryPrefix: true,
      encode: false,
      arrayFormat: "comma",
    });
    navigate(`${path}${queryString}`);
  };

  return {
    setParams,
    search,
    ...params,
  };
};
