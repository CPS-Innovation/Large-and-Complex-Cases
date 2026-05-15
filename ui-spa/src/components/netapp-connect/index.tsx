import { useState, useEffect } from "react";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import { useApi } from "../../common/hooks/useApi";
import { getConnectNetAppFolders } from "../../apis/gateway-api";
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
    state?: {
      searchQueryString: string;
    };
  } = useLocation();

  const { searchQueryString } = state || {};

  const [operationName, setOperationName] = useState<string | null>("");
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
    navigate(`/case/${caseId}/netapp-connect/confirmation`, {
      state: {
        isRouteValid: true,
        operationName,
        caseId,
        searchQueryString,
        backLinkUrl: `/case/${caseId}/netapp-connect?operation-name=${operationName}`,
        selectedWorkspace: {
          folderPath: path,
        },
      },
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
