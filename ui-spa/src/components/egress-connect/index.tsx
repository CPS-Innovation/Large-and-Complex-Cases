import { useEffect, useState, useMemo, useCallback } from "react";
import { useApi } from "../../common/hooks/useApi";
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
  const [initialLocationState, setInitialLocationState] = useState<{
    searchQueryString: string;
    netappFolderPath: boolean;
  }>();
  const [selectedFolderId, setSelectedFolderId] = useState("");
  const [formDataErrorText, setFormDataErrorText] = useState("");
  const [formValue, setFormValue] = useState("");

  const egressSearchApi = useApi(
    getEgressSearchResults,
    [`workspace-name=${workspaceName}`],
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
    if (location.state?.searchQueryString) {
      setInitialLocationState({
        searchQueryString: location.state?.searchQueryString,
        netappFolderPath: location.state?.netappFolderPath,
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
    setSelectedFolderId(id);
    navigate(`/case/${caseId}/egress-connect/confirmation`);
  };
  const handleContinue = async (connect: boolean) => {
    if (!connect) {
      navigate(
        `/case/${caseId}/egress-connect?workspace-name=${workspaceName}`,
      );
      return;
    }
    try {
      await connectEgressWorkspace({
        workspaceId: selectedFolderId,
        caseId: caseId!,
      });

      console.log(
        "netappFolderPath>>>",
        initialLocationState?.netappFolderPath,
      );

      if (!initialLocationState?.netappFolderPath)
        navigate(
          `/case/${caseId}/netapp-connect?operation-name=${workspaceName}`,
          {
            state: {
              searchQueryString: initialLocationState?.searchQueryString,
            },
          },
        );
      else navigate(`/case/${caseId}/case-overview/transfer-material`);
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
    } catch (e) {
      navigate(`/case/${caseId}/egress-connect/error`);
    }
  };

  const validateRoute = useCallback(() => {
    let validRoute = true;
    if (
      location.pathname.endsWith("/egress-connect") &&
      initialLocationState?.netappFolderPath === undefined &&
      location.state?.netappFolderPath === undefined
    ) {
      validRoute = false;
    }
    if (location.pathname.endsWith("/confirmation") && !selectedFolderId)
      validRoute = false;
    if (location.pathname.endsWith("/error") && !selectedFolderId)
      validRoute = false;
    if (!validRoute) navigate(`/`);
  }, [location, initialLocationState, navigate, selectedFolderId]);

  useEffect(() => {
    validateRoute();
  }, [location, validateRoute]);

  const selectedWorkSpaceName = useMemo(() => {
    if (egressSearchApi.status !== "succeeded") return "";
    return (
      egressSearchApi.data?.find((data) => data.id === selectedFolderId)
        ?.name ?? ""
    );
  }, [egressSearchApi, selectedFolderId]);

  if (location.pathname.endsWith("/error"))
    return (
      <div className="govuk-width-container">
        <EgressConnectFailurePage
          backLinkUrl={`/case/${caseId}/egress-connect?workspace-name=${workspaceName}`}
        />
      </div>
    );
  if (location.pathname.endsWith("/confirmation"))
    return (
      <div className="govuk-width-container">
        <EgressConnectConfirmationPage
          selectedWorkspaceName={selectedWorkSpaceName}
          backLinkUrl={`/case/${caseId}/egress-connect?workspace-name=${workspaceName}`}
          handleContinue={handleContinue}
        />
      </div>
    );
  return (
    <div className="govuk-width-container">
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
    </div>
  );
};

export default EgressPage;
