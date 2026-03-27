import { useNavigate, useSearchParams } from "react-router-dom";
import { type CaseSearchParams } from "../types/CaseSearchParams";

const useSearchNavigation = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const updateSearchParams = (params: CaseSearchParams) => {
    setSearchParams(params);
  };

  const navigateWithParams = (params: CaseSearchParams) => {
    const queryString = new URLSearchParams(params);

    navigate(`/search-results?${queryString}`);
  };

  const validateSearchParams = (params: URLSearchParams): CaseSearchParams => {
    const validKeys: (keyof CaseSearchParams)[] = [
      "urn",
      "operation-name",
      "defendant-name",
      "area",
    ];
    const validParams: Partial<CaseSearchParams> = {};
    for (const [key, value] of params.entries()) {
      if (validKeys.includes(key as keyof CaseSearchParams)) {
        validParams[key as keyof CaseSearchParams] = value;
      }
    }

    return validParams as CaseSearchParams;
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
