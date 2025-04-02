import { useEffect, useState, useMemo } from "react";
import { useApi } from "../../common/hooks/useApi";
import { RawApiResult } from "../../common/types/ApiResult";
import { EgressSearchResultData } from "../../common/types/EgressSearchResponse";
import { getEgressSearchResults } from "../../apis/gateway-api";
import { useNavigate, useSearchParams } from "react-router-dom";
import EgressSearchPage from "./EgressSearchPage";
import EgressConnectConfirmationPage from "./EgressConnectConfirmationPage";
import EgressConnectFailurePage from "./EgressConnectFailurePage";

const EgressPage = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const workspaceName = useMemo(
    () => searchParams.get("workspace-name") ?? "",
    [searchParams],
  );

  const [formValue, setFormValue] = useState(workspaceName);
  const [selectedFolderId, setSelectedFolderId] = useState("");
  const egressSearchApiResults: RawApiResult<EgressSearchResultData> = useApi(
    getEgressSearchResults,
    [`workspace-name=${workspaceName}`],
    true,
  );

  const handleSearch = () => {
    setSearchParams({ "workspace-name": formValue });
    // navigate("/egress-connect");
  };

  const handleFormChange = (value: string) => {
    setFormValue(value);
  };

  const handleConnectFolder = (id: string) => {
    setSelectedFolderId(id);
    // navigate(`/egress-connect?id=${id}`);
  };
  const handleContinue = (connect: boolean) => {
    if (!connect) {
      setSelectedFolderId("");
      return;
    }
    //make api call-[]
  };

  return (
    <div className="govuk-width-container">
      {!selectedFolderId && (
        <EgressSearchPage
          searchValue={formValue}
          egressSearchApiResults={egressSearchApiResults}
          handleFormChange={handleFormChange}
          handleSearch={handleSearch}
          handleConnectFolder={handleConnectFolder}
        />
      )}
      {selectedFolderId && (
        <EgressConnectConfirmationPage
          backLinkUrl={`/egress-connect?workspace-name=${workspaceName}`}
          handleContinue={handleContinue}
          // handleBack={handleBack}
        />
      )}

      {/* <EgressConnectFailurePage
        backLinkUrl={`/egress-connect?workspace-name=${workspaceName}`}
      /> */}
    </div>
  );
};

export default EgressPage;
