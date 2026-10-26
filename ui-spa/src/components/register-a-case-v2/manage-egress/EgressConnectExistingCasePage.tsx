import { useEffect, useState, useContext, useMemo } from "react";
import {
  getEgressSearchResults,
  connectEgressWorkspace,
} from "../../../apis/gateway-api";
import { useNavigate, useSearchParams, useParams } from "react-router";
import { getUrlSearchParam } from "../../../common/utils/getUrlSearchParam";
import { useQuery } from "@tanstack/react-query";
import { MainStateContext } from "../../../providers/MainStateProvider";
import EgressSearchPage from "./EgressSearchPageV2";

const EgressConnectExistingCasePage = () => {
  const { dispatch } = useContext(MainStateContext);
  const navigate = useNavigate();
  const { caseId } = useParams();
  const [searchParams] = useSearchParams();

  const [workspaceName, setWorkspaceName] = useState("");
  const [formDataErrorText, setFormDataErrorText] = useState("");
  const [formValue, setFormValue] = useState("");

  const { data: egressSearchResults, isLoading: isEgressSearchResultLoading } =
    useQuery({
      queryKey: [workspaceName],
      queryFn: () => getEgressSearchResults(workspaceName),
      retry: false,
      enabled: !!workspaceName,
      throwOnError: true,
      staleTime: 0,
      gcTime: 0,
    });

  useEffect(() => {
    const name = searchParams.get("workspace-name");

    if (!name) {
      setFormDataErrorText(
        "egress folder name  and case Id should not be empty",
      );
      return;
    }
    setWorkspaceName(name);
    setFormValue(name);
  }, [searchParams]);

  const backLinkUrl = useMemo(
    () =>
      `/case-registration/${caseId}/egress-connect-search-results?${getUrlSearchParam("workspace-name", formValue)}`,
    [caseId, formValue],
  );

  const handleSearch = () => {
    if (!formValue) {
      setFormDataErrorText("egress folder name should not be empty");
      return;
    }
    setFormDataErrorText("");

    void navigate(
      `/case-registration/${caseId}/egress-connect-search-results?${getUrlSearchParam("workspace-name", formValue)}`,
    );
  };

  const handleFormChange = (value: string) => {
    setFormValue(value);
  };

  const handleConnectFolder = async (id: string) => {
    const selectedWorkspace = egressSearchResults?.find(
      (data) => data.id === id,
    );

    console.log("selectedWorkspace>>", selectedWorkspace);
    if (!selectedWorkspace) return;

    try {
      if (selectedWorkspace.id && caseId) {
        await connectEgressWorkspace({
          workspaceId: selectedWorkspace.id,
          workspaceName: selectedWorkspace.name,
          caseId: caseId,
        });

        void navigate(
          `/case-registration/${caseId}/egress-connect-confirmation`,
        );
      }
    } catch (error) {
      if (error) {
        dispatch({
          type: "SET_CASE_REGISTRATION_EGRESS_CONNECT_FAILURE_PAGE",
          payload: { backLinkUrl },
        });
        void navigate(`/case-registration/egress-connect-error`);
      }
    }
  };

  return (
    <EgressSearchPage
      backLinkUrl={"/case-registration/manage-set-up-egress"}
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

export default EgressConnectExistingCasePage;
