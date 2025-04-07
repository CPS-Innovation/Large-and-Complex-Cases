import { useEffect, useState } from "react";
import { useApiNew } from "../../common/hooks/useApiNew";
import {
  getEgressSearchResults,
  connectEgressWorkspace,
} from "../../apis/gateway-api";
import {
  useNavigate,
  useSearchParams,
  useLocation,
  useParams,
} from "react-router-dom";
import EgressSearchPage from "./EgressSearchPage";
import EgressConnectConfirmationPage from "./EgressConnectConfirmationPage";
import EgressConnectFailurePage from "./EgressConnectFailurePage";

const EgressPage = () => {
  const navigate = useNavigate();
  const { caseId } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const location = useLocation();

  const [workspaceName, setWorkspaceName] = useState("");
  const [selectedFolderId, setSelectedFolderId] = useState("");
  const [formDataErrorText, setFormDataErrorText] = useState("");
  const [formValue, setFormValue] = useState("");

  useEffect(() => {
    console.log("mounting component>>>>");
    if (location.pathname.endsWith("/egress-connect")) {
      const name = searchParams.get("workspace-name");
      if (!name) {
        setFormDataErrorText("egress folder name should not be empty");
        return;
      }
      setWorkspaceName(name);
      setFormValue(name);
    }
  }, [searchParams]);

  const egressSearchApi = useApiNew(
    getEgressSearchResults,
    [`workspace-name=${workspaceName}`],
    !!workspaceName,
  );

  const egressConnectApi = useApiNew(
    connectEgressWorkspace,
    [{ workspaceId: selectedFolderId, caseId: "1" }],
    false,
  );

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
    setSelectedFolderId(id);
    navigate(`/case/${caseId}/egress-connect/confirmation`);
  };
  const handleContinue = async (connect: boolean) => {
    if (!connect) {
      setSelectedFolderId("");
      return;
    }
    egressConnectApi.refetch();
  };

  useEffect(() => {
    if (egressConnectApi.error)
      navigate(`/case/${caseId}/egress-connect/error`);
  }, [egressConnectApi.error]);

  useEffect(() => {
    validateRoute();
  });

  const validateRoute = () => {
    let validRoute = true;
    if (location.pathname.includes("/error") && !egressConnectApi.error)
      validRoute = false;
    if (location.pathname.includes("/confirmation") && !selectedFolderId)
      validRoute = false;
    if (!validRoute)
      navigate(
        `/case/${caseId}/egress-connect?workspace-name=${workspaceName}`,
      );
  };

  if (location.pathname.includes("/error"))
    return (
      <div className="govuk-width-container">
        <EgressConnectFailurePage
          backLinkUrl={`/case/${caseId}/egress-connect?workspace-name=${workspaceName}`}
        />
      </div>
    );
  if (location.pathname.includes("/confirmation"))
    return (
      <div className="govuk-width-container">
        <EgressConnectConfirmationPage
          backLinkUrl={`/case/${caseId}/egress-connect?workspace-name=${workspaceName}`}
          handleContinue={handleContinue}
        />
      </div>
    );
  return (
    <div className="govuk-width-container">
      <EgressSearchPage
        searchValue={formValue}
        formDataErrorText={formDataErrorText}
        egressSearchApi={egressSearchApi}
        handleFormChange={handleFormChange}
        handleSearch={handleSearch}
        handleConnectFolder={handleConnectFolder}
      />
    </div>
  );
};

export default EgressPage;
