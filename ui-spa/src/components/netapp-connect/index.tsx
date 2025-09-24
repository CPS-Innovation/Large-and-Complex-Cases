import { useState, useEffect, useCallback } from "react";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import NetAppConnectConfirmationPage from "./NetAppConnectConfirmationPage";
import NetAppConnectFailurePage from "./NetAppConnectFailurePage";
import { useApi } from "../../common/hooks/useApi";
import {
  getConnectNetAppFolders,
  connectNetAppFolder,
} from "../../apis/gateway-api";
import { getFolderNameFromPath } from "../../common/utils/getFolderNameFromPath";
import {
  useNavigate,
  useSearchParams,
  useLocation,
  useParams,
} from "react-router-dom";

const NetAppPage = () => {
  const navigate = useNavigate();
  const { caseId } = useParams();
  const [searchParams] = useSearchParams();
  const location = useLocation();

  const [operationName, setOperationName] = useState<string | null>("");
  const [initialLocationState, setInitialLocationState] = useState<{
    searchQueryString: string;
  }>();
  const [selectedFolderPath, setSelectedFolderPath] = useState("");
  const [rootFolderPath, setRootFolderPath] = useState("");

  const netAppFolderApiResults = useApi(
    getConnectNetAppFolders,
    [operationName, rootFolderPath],
    false,
  );

  useEffect(() => {
    if (operationName) netAppFolderApiResults.refetch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rootFolderPath, operationName]);

  useEffect(() => {
    if (location.pathname.endsWith("/netapp-connect")) {
      const opName = searchParams.get("operation-name");
      if (!opName) {
        setOperationName(null);
        return;
      }
      setOperationName(opName);
    }
  }, [location, setOperationName, searchParams]);

  useEffect(() => {
    if (netAppFolderApiResults.status === "failed")
      throw new Error(`${netAppFolderApiResults.error}`);
  }, [netAppFolderApiResults]);

  const handleGetFolderContent = (path: string) => {
    setRootFolderPath(path);
  };

  const handleConnectFolder = (path: string) => {
    setSelectedFolderPath(path);
    navigate(`/case/${caseId}/netapp-connect/confirmation`);
  };

  const handleContinue = async (connect: boolean) => {
    if (!connect) {
      navigate(
        `/case/${caseId}/netapp-connect?operation-name=${operationName}`,
      );
      return;
    }
    try {
      await connectNetAppFolder({
        operationName: operationName!,
        folderPath: selectedFolderPath,
        caseId: caseId!,
      });
      navigate(`/case/${caseId}/case-management`);
    } catch (error) {
      if (error) navigate(`/case/${caseId}/netapp-connect/error`);
    }
  };
  useEffect(() => {
    if (location.state?.searchQueryString !== undefined) {
      setInitialLocationState({
        searchQueryString: location.state?.searchQueryString,
      });
    }
  }, [location]);

  const validateRoute = useCallback(() => {
    let validRoute = true;
    if (operationName === null) validRoute = false;
    if (
      location.pathname.endsWith("/netapp-connect") &&
      initialLocationState?.searchQueryString === undefined &&
      location.state?.searchQueryString === undefined
    ) {
      validRoute = false;
    }
    if (location.pathname.endsWith("/confirmation") && !selectedFolderPath)
      validRoute = false;

    if (location.pathname.endsWith("/error") && !selectedFolderPath)
      validRoute = false;
    if (!validRoute) navigate(`/`);
  }, [
    location,
    initialLocationState,
    navigate,
    selectedFolderPath,
    operationName,
  ]);

  useEffect(() => {
    if (location.pathname) validateRoute();
  }, [location, validateRoute]);

  useEffect(() => {
    return () => {
      window.history.replaceState({}, "");
    };
  }, []);

  if (location.pathname.endsWith("/error"))
    return (
      <NetAppConnectFailurePage
        backLinkUrl={`/case/${caseId}/netapp-connect?operation-name=${operationName}`}
      />
    );
  if (location.pathname.endsWith("/confirmation"))
    return (
      <NetAppConnectConfirmationPage
        selectedFolderName={getFolderNameFromPath(selectedFolderPath)}
        backLinkUrl={`/case/${caseId}/netapp-connect?operation-name=${operationName}`}
        handleContinue={handleContinue}
      />
    );

  return (
    <NetAppFolderResultsPage
      backLinkUrl={
        initialLocationState?.searchQueryString
          ? `/search-results?${initialLocationState?.searchQueryString}`
          : "/search-results"
      }
      rootFolderPath={rootFolderPath}
      netAppFolderApiResults={netAppFolderApiResults}
      handleConnectFolder={handleConnectFolder}
      handleGetFolderContent={handleGetFolderContent}
    />
  );
};

export default NetAppPage;
