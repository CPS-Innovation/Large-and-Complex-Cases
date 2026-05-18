import { useState, useEffect } from "react";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import { useApi } from "../../common/hooks/useApi";
import { getConnectNetAppFolders } from "../../apis/gateway-api";
import { SharedDriveConnectRouteState } from "../../common/types/SharedDriveConnectRouteState";
import { SharedDriveConnectConfirmationRouteState } from "../../common/types/SharedDriveConnectConfirmationRouteState";
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
  const {
    state,
  }: {
    state: SharedDriveConnectRouteState;
  } = useLocation();

  const { searchQueryString, netappRootFolderPath } = state;

  const [operationName, setOperationName] = useState<string | null>("");
  const [rootFolderPath, setRootFolderPath] = useState(
    netappRootFolderPath ?? "",
  );

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
    const payload: SharedDriveConnectConfirmationRouteState = {
      isRouteValid: true,
      operationName: operationName!,
      caseId: caseId!,
      searchQueryString: searchQueryString,
      netappRootFolderPath: rootFolderPath,
      backLinkUrl: `/case/${caseId}/netapp-connect?operation-name=${operationName}`,
      selectedWorkspace: {
        folderPath: path,
      },
    };
    navigate(`/case/${caseId}/netapp-connect/confirmation`, {
      state: payload,
    });
  };

  return (
    <NetAppFolderResultsPage
      backLinkUrl={`/search-results?${searchQueryString}`}
      rootFolderPath={rootFolderPath}
      netAppFolderApiResults={netAppFolderApiResults}
      handleConnectFolder={handleConnectFolder}
      handleGetFolderContent={handleGetFolderContent}
    />
  );
};

export default NetAppPage;
