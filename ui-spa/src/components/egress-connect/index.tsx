import { useEffect, useState } from "react";
import { getEgressSearchResults } from "../../apis/gateway-api";
import {
  useNavigate,
  useSearchParams,
  useLocation,
  useParams,
} from "react-router-dom";
import { EgressConnectRouteState } from "../../common/types/EgressConnectRouteState";
import { EgressConnectConfirmationRouteState } from "../../common/types/EgressConnectConfirmationRouteState";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import { useQuery } from "@tanstack/react-query";
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
    state: EgressConnectRouteState;
  } = useLocation();

  const { isNetAppConnected, searchQueryString } = state;

  const { data: egressSearchResults, isLoading: isEgressSearchResultLoading } =
    useQuery({
      queryKey: [`egressSearch-${workspaceName}`],
      queryFn: () => getEgressSearchResults(workspaceName),
      retry: false,
      enabled: true,
      throwOnError: true,
    });

  useEffect(() => {
    const name = searchParams.get("workspace-name");
    if (!name) {
      setFormDataErrorText("egress folder name should not be empty");
      return;
    }
    setWorkspaceName(name);
    setFormValue(name);
  }, [searchParams]);

  const handleSearch = () => {
    if (!formValue) {
      setFormDataErrorText("egress folder name should not be empty");
      return;
    }
    setFormDataErrorText("");

    const payload: EgressConnectRouteState = {
      isRouteValid: true,
      searchQueryString,
      isNetAppConnected,
    };
    navigate(
      `/case/${caseId}/egress-connect?${getUrlSearchParam("workspace-name", formValue)}`,
      {
        state: payload,
      },
    );
  };

  const handleFormChange = (value: string) => {
    setFormValue(value);
  };

  const handleConnectFolder = (id: string) => {
    const selectedWorkSpace = egressSearchResults?.find(
      (data) => data.id === id,
    );
    if (!selectedWorkSpace) return;

    const payload: EgressConnectConfirmationRouteState = {
      isRouteValid: true,
      backLinkUrl: `/case/${caseId}/egress-connect?${getUrlSearchParam("workspace-name", formValue)}`,
      caseId: caseId!,
      searchQueryString,
      isNetAppConnected,
      selectedWorkspace: {
        id: selectedWorkSpace?.id,
        name: selectedWorkSpace?.name,
      },
    };
    navigate(`/case/${caseId}/egress-connect/confirmation`, {
      state: payload,
    });
  };

  return (
    <EgressSearchPage
      backLinkUrl={`/search-results?${searchQueryString}`}
      workspaceName={workspaceName}
      searchValue={formValue}
      formDataErrorText={formDataErrorText}
      egressSearchResults={egressSearchResults}
      isEgressSearchResultLoading={isEgressSearchResultLoading}
      handleFormChange={handleFormChange}
      handleSearch={handleSearch}
      handleConnectFolder={handleConnectFolder}
    />
  );
};

export default EgressPage;
