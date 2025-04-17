import { useState, useEffect } from "react";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import { useApi } from "../../common/hooks/useApi";
import { getNetAppFolders } from "../../apis/gateway-api";
import {
  useNavigate,
  useSearchParams,
  useLocation,
  useParams,
} from "react-router-dom";

const NetAppPage = () => {
  const navigate = useNavigate();
  const { caseId } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const location = useLocation();

  const [operationName, setOperationName] = useState("");
  const [initialLocationState, setInitialLocationState] = useState<{
    searchQueryString: string;
    netappFolderPath: boolean;
  }>();
  const [selectedFolderPath, setSelectedFolderPath] = useState("");
  const [rootFolderPath, setRootFolderPath] = useState("");

  const netAppFolderApiResults = useApi(
    getNetAppFolders,
    [operationName, rootFolderPath],
    false,
  );

  useEffect(() => {
    netAppFolderApiResults.refetch();
  }, [rootFolderPath]);

  useEffect(() => {
    if (location.pathname.endsWith("/netapp-connect")) {
      const operationName = searchParams.get("operation-name");
      if (!operationName) {
        return;
      }
      setOperationName("");
    }
    return () => {
      window.history.replaceState({}, "");
    };
  }, []);

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

  return (
    <NetAppFolderResultsPage
      backLinkUrl={
        initialLocationState?.searchQueryString
          ? `/search-results?${initialLocationState?.searchQueryString}`
          : "/search-results"
      }
      netAppFolderApiResults={netAppFolderApiResults}
      handleConnectFolder={handleConnectFolder}
      handleGetFolderContent={handleGetFolderContent}
    />
  );
};

export default NetAppPage;
