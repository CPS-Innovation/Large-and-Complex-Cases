import { useEffect, useState } from "react";
import { useApi } from "../../common/hooks/useApi";
import { useApiNew } from "../../common/hooks/useApiNew";
import { RawApiResult } from "../../common/types/ApiResult";
import { EgressSearchResultData } from "../../common/types/EgressSearchResponse";
import {
  getEgressSearchResults,
  connectEgressWorkspace,
} from "../../apis/gateway-api";
import { useNavigate, useSearchParams, useLocation } from "react-router-dom";
import EgressSearchPage from "./EgressSearchPage";
import EgressConnectConfirmationPage from "./EgressConnectConfirmationPage";
import EgressConnectFailurePage from "./EgressConnectFailurePage";

const EgressPage = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const location = useLocation();

  const [workspaceName, setWorkspaceName] = useState("");
  const [formValue, setFormValue] = useState("");

  useEffect(() => {
    console.log("mounting component>>>>");
    if (location.pathname === "/egress-connect") {
      const name = searchParams.get("workspace-name") ?? "";
      setWorkspaceName(name);
      setFormValue(name);
    }
  }, [searchParams]);

  const [selectedFolderId, setSelectedFolderId] = useState("");
  const egressSearchApiResults: RawApiResult<EgressSearchResultData> = useApi(
    getEgressSearchResults,
    [`workspace-name=${workspaceName}`],
  );

  const egressConnectApi = useApiNew(
    connectEgressWorkspace,
    [{ workspaceId: selectedFolderId, caseId: "1" }],
    false,
  );

  const handleSearch = () => {
    setSearchParams({ "workspace-name": formValue });
  };

  const handleFormChange = (value: string) => {
    setFormValue(value);
  };

  const handleConnectFolder = (id: string) => {
    setSelectedFolderId(id);
    navigate(`/egress-connect/confirmation`);
  };
  const handleContinue = async (connect: boolean) => {
    if (!connect) {
      setSelectedFolderId("");
      return;
    }
    egressConnectApi.refetch();
  };

  useEffect(() => {
    if (egressConnectApi.error) navigate("/egress-connect/error");
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
    if (!validRoute) navigate("/");
  };

  if (location.pathname.includes("/error"))
    return (
      <div className="govuk-width-container">
        <EgressConnectFailurePage
          backLinkUrl={`/egress-connect?workspace-name=${workspaceName}`}
        />
      </div>
    );
  if (location.pathname.includes("/confirmation"))
    return (
      <div className="govuk-width-container">
        <EgressConnectConfirmationPage
          backLinkUrl={`/egress-connect?workspace-name=${workspaceName}`}
          handleContinue={handleContinue}
          // handleBack={handleBack}
        />
      </div>
    );
  return (
    <div className="govuk-width-container">
      <EgressSearchPage
        searchValue={formValue}
        egressSearchApiResults={egressSearchApiResults}
        handleFormChange={handleFormChange}
        handleSearch={handleSearch}
        handleConnectFolder={handleConnectFolder}
      />
    </div>
  );
};

export default EgressPage;
