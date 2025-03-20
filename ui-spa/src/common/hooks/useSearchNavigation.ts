import { useNavigate, useSearchParams } from "react-router-dom";

export type SearchParamsType = {
  urn?: string;
  "operation-name"?: string;
  "defendant-name"?: string;
  area?: string;
};

const useSearchNavigation = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const updateSearchParams = (params: SearchParamsType) => {
    setSearchParams(params);
  };

  const navigateWithParams = (params: SearchParamsType) => {
    const queryString = new URLSearchParams(Object.entries(params)).toString();

    navigate(`/search-results?${queryString}`);
  };

  const validateSearchParams = (params: URLSearchParams): SearchParamsType => {
    const validKeys: (keyof SearchParamsType)[] = [
      "urn",
      "operation-name",
      "defendant-name",
      "area",
    ];
    const validParams: Partial<SearchParamsType> = {};
    for (let [key, value] of params.entries()) {
      if (validKeys.includes(key as keyof SearchParamsType)) {
        validParams[key as keyof SearchParamsType] = value;
      }
    }

    return validParams as SearchParamsType;
  };
  const filteredSearchParams = validateSearchParams(searchParams);
  return {
    updateSearchParams,
    navigateWithParams,
    searchParams: filteredSearchParams,
    queryString: new URLSearchParams(filteredSearchParams).toString(),
  };
};

export default useSearchNavigation;
