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
  const [searchParams] = useSearchParams();

  const [workspaceName, setWorkspaceName] = useState("");
  const [formDataErrorText, setFormDataErrorText] = useState("");
  const [formValue, setFormValue] = useState("");
  const {
    state,
  }: {
    state?: {
      isNetAppConnected: boolean;
      searchQueryString: string;
    };
  } = useLocation();

  const { isNetAppConnected, searchQueryString } = state || {};

  const egressSearchApi = useApi(
    getEgressSearchResults,
    [workspaceName],
    false,
  );

  useEffect(() => {
    const name = searchParams.get("workspace-name");
    if (!name) {
      setFormDataErrorText("egress folder name should not be empty");
      return;
    }
    setWorkspaceName(name);
    setFormValue(name);
  }, [searchParams]);

  useEffect(() => {
    if (workspaceName) egressSearchApi.refetch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [workspaceName]);

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
    navigate(`/case/${caseId}/egress-connect?workspace-name=${formValue}`, {
      state: {
        isRouteValid: true,
        searchQueryString,
        isNetAppConnected,
      },
    });
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
        backLinkUrl: `/case/${caseId}/egress-connect?workspace-name=${workspaceName}`,
        caseId,
        searchQueryString,
        isNetAppConnected: isNetAppConnected,
        selectedWorkspace: {
          id: selectedWorkSpace?.id,
          name: selectedWorkSpace?.name,
        },
      },
    });
  };

  return (
    <EgressSearchPage
      backLinkUrl={`/search-results?${searchQueryString}`}
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
