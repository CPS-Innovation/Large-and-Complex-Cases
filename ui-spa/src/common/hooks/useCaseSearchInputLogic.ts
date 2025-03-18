import { useState, KeyboardEvent } from "react";
// import { validateSearchKey } from "../logic/validate-searchKey";
import { CaseSearchQueryParams } from "../types/CaseSearchQueryParams";

export const useCaseSearchInputLogic = ({
  searchKeyFromSearchParams,
  setParams,
  search,
}: {
  searchKeyFromSearchParams: string | undefined;
  setParams: (params: Partial<CaseSearchQueryParams>) => void;
  search?: string;
}) => {
  const [searchKey, setSearchKey] = useState(searchKeyFromSearchParams || "");
  const [isError, setIsError] = useState(false);

  const allowedParams = ["redactionLog"];
  const queryParams = new URLSearchParams(search);
  const getQueryParamsObject = (queryParams: URLSearchParams) => {
    const params = {} as any;
    for (const [key, value] of queryParams.entries()) {
      if (allowedParams.includes(key)) {
        params[key] = value;
      }
    }
    return params;
  };
  const paramsObject = getQueryParamsObject(queryParams);

  const handleChange = (val: string) => {
    console.log("vak>>>", val);
    setSearchKey(val.toUpperCase());
  };

  const validateSearchKey = (searchKey: string) => {
    return !!searchKey;
  };
  const handleSubmit = () => {
    const isValid = validateSearchKey(searchKey);
    setIsError(!isValid);
    if (isValid) {
      setParams({ ...paramsObject, search: searchKey });
    }
  };

  const handleKeyPress = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleSubmit();
    }
  };

  return {
    handleChange,
    handleKeyPress,
    handleSubmit,
    searchKey,
    isError,
  };
};
