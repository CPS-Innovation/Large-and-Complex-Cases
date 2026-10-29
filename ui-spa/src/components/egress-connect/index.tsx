import { useEffect, useState, useContext } from "react";
import {
  getEgressSearchResults,
  connectEgressWorkspace,
} from "../../apis/gateway-api";
import { useNavigate, useSearchParams, useParams } from "react-router";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import { useQuery } from "@tanstack/react-query";
import { MainStateContext } from "../../providers/MainStateProvider";
import EgressSearchPage from "./EgressSearchPage";

const EgressPage = () => {
  const { state, dispatch } = useContext(MainStateContext);
  const navigate = useNavigate();
  const { caseId: paramCaseId } = useParams();
  const [searchParams] = useSearchParams();
  const { urn, caseId: caseRegistrationCaseId } =
    state.appData.caseRegistrationCaseDetails ?? {};

  const [workspaceName, setWorkspaceName] = useState("");
  const [formDataErrorText, setFormDataErrorText] = useState("");
  const [formValue, setFormValue] = useState("");
  const [disableBtns, setDisableBtns] = useState(false);

  const isRegistrationFlow = location.pathname.includes("/case-registration/");
  const caseId = isRegistrationFlow ? caseRegistrationCaseId : paramCaseId;

  useEffect(() => {
    if (isRegistrationFlow && !caseRegistrationCaseId) {
      void navigate("/", { replace: true });
    }
  }, [isRegistrationFlow, caseRegistrationCaseId, navigate]);

  const { isNetAppConnected, searchQueryString } =
    state.appData.connectEgressPage;

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
    const basePath = isRegistrationFlow
      ? `/case-registration/egress-connect-search-results`
      : `/case/${caseId}/egress-connect`;

    navigate(`${basePath}?${getUrlSearchParam("workspace-name", formValue)}`);
  };

  const handleFormChange = (value: string) => {
    setFormValue(value);
  };

  const handleConnectFolder = async (id: string) => {
    const selectedWorkspace = egressSearchResults?.find(
      (data) => data.id === id,
    );
    if (!selectedWorkspace) return;
    setDisableBtns(true);
    try {
      if (selectedWorkspace.id && caseId) {
        await connectEgressWorkspace({
          workspaceId: selectedWorkspace.id,
          workspaceName: selectedWorkspace.name,
          caseId: `${caseId}`,
        });
        if (isRegistrationFlow) {
          void navigate(
            `/case-registration/egress-connect-confirmation?${getUrlSearchParam("egress-workspace-name", selectedWorkspace.name)}`,
          );
          return;
        }
        if (!isNetAppConnected) {
          dispatch({
            type: "SET_SHARED_DRIVE_CONNECT_PAGE",
            payload: {
              searchQueryString,
              netappRootFolderPath: "",
              accessibleRoots: null,
            },
          });
          navigate(
            `/case/${caseId}/netapp-connect?${getUrlSearchParam("operation-name", selectedWorkspace.name)}`,
          );
          return;
        }
        void navigate(`/case/${caseId}/case-management`);
      }
    } catch (error) {
      if (error) {
        const basePath = isRegistrationFlow
          ? `/case-registration/egress-connect-search-results`
          : `/case/${caseId}/egress-connect`;
        dispatch({
          type: "SET_EGRESS_CONNECT_FAILURE_PAGE",
          payload: {
            backLinkUrl: `${basePath}?${getUrlSearchParam("workspace-name", formValue)}`,
          },
        });
        if (isRegistrationFlow) {
          void navigate(`/case-registration/egress-connect-error`);
          return;
        }
        void navigate(`/case/${caseId}/egress-connect/error`);
      }
    }
  };

  return (
    <EgressSearchPage
      backLinkUrl={
        isRegistrationFlow
          ? `/case-registration/manage-set-up-egress?${getUrlSearchParam("urn", urn)}`
          : `/search-results?${searchQueryString}`
      }
      workspaceName={workspaceName}
      searchValue={formValue}
      formDataErrorText={formDataErrorText}
      egressSearchResults={egressSearchResults}
      isEgressSearchResultLoading={isEgressSearchResultLoading}
      handleFormChange={handleFormChange}
      handleSearch={handleSearch}
      disableBtns={isEgressSearchResultLoading || disableBtns}
      handleConnectFolder={handleConnectFolder}
    />
  );
};

export default EgressPage;
