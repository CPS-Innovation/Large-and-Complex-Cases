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
    // Remove undefined values to avoid unnecessary empty params
    const filteredParams = Object.fromEntries(
      Object.entries(params).filter(([_, value]) => value),
    );

    setSearchParams(filteredParams);
  };

  const navigateWithParams = (params: SearchParamsType) => {
    const queryString = new URLSearchParams(
      Object.entries(params).filter(([_, value]) => value),
    ).toString();

    console.log("queryString>>", queryString);
    queryString.replace("operationName", "operation-name");
    queryString.replace("defendantName", "defendant-name");

    navigate(`/search-results?${queryString}`);
  };

  return { updateSearchParams, navigateWithParams, searchParams };
};

export default useSearchNavigation;
