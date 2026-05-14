import { useEffect, useState } from "react";
import { useApi } from "../../common/hooks/useApi";
import { getEgressSearchResults } from "../../apis/gateway-api";
import {
  useNavigate,
  useSearchParams,
  useLocation,
  useParams,
} from "react-router-dom";
import EgressSearchPage from "./EgressSearchPage";

const EgressPage = () => {
  const navigate = useNavigate();
  const { caseId } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const location = useLocation();

  const [workspaceName, setWorkspaceName] = useState("");
  const [initialLocationState, setInitialLocationState] = useState<{
    searchQueryString: string;
    isNetAppConnected: boolean;
  }>();
  const [formDataErrorText, setFormDataErrorText] = useState("");
  const [formValue, setFormValue] = useState("");

  const egressSearchApi = useApi(
    getEgressSearchResults,
    [workspaceName],
    false,
  );

  useEffect(() => {
    if (location.pathname.endsWith("/egress-connect")) {
      const name = searchParams.get("workspace-name");
      if (!name) {
        setFormDataErrorText("egress folder name should not be empty");
        return;
      }
      setWorkspaceName(name);
      setFormValue(name);
    }
  }, [searchParams, location.pathname]);

  useEffect(() => {
    if (
      location.state?.searchQueryString !== undefined ||
      location.state?.isNetAppConnected !== undefined
    ) {
      setInitialLocationState({
        searchQueryString: location.state?.searchQueryString,
        isNetAppConnected: location.state?.isNetAppConnected,
      });
    }
  }, [location]);

  useEffect(() => {
    if (workspaceName) egressSearchApi.refetch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [workspaceName]);

  useEffect(() => {
    return () => {
      window.history.replaceState({}, "");
    };
  }, []);

  useEffect(() => {
    if (egressSearchApi.status === "failed")
      throw new Error(`${egressSearchApi.error}`);
  }, [egressSearchApi]);

  const handleSearch = () => {
    if (!formValue) {
      setFormDataErrorText("egress folder name should not be empty");
      return;
    }
    setFormDataErrorText("");
    setSearchParams({ "workspace-name": formValue });
  };

  const handleFormChange = (value: string) => {
    setFormValue(value);
  };

  const handleConnectFolder = (id: string) => {
    const selectedWorkSpace = egressSearchApi.data?.find(
      (data) => data.id === id,
    );
    if (!selectedWorkSpace) return;
    navigate(`/case/${caseId}/egress-connect/confirmation`, {
      state: {
        isRouteValid: true,
        selectedWorkspace: {
          id: selectedWorkSpace?.id,
          name: selectedWorkSpace?.name,
        },
      },
    });
  };

  return (
    <EgressSearchPage
      backLinkUrl={
        initialLocationState?.searchQueryString
          ? `/search-results?${initialLocationState?.searchQueryString}`
          : "/search-results"
      }
      workspaceName={workspaceName}
      searchValue={formValue}
      formDataErrorText={formDataErrorText}
      egressSearchApi={egressSearchApi}
      handleFormChange={handleFormChange}
      handleSearch={handleSearch}
      handleConnectFolder={handleConnectFolder}
    />
  );
};

export default EgressPage;
