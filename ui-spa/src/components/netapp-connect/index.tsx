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

  const [folderPath, setFolderPath] = useState("");
  const [initialLocationState, setInitialLocationState] = useState<{
    searchQueryString: string;
    netappFolderPath: boolean;
  }>();
  const [selectedFolderId, setSelectedFolderId] = useState("");

  const netAppFolderApiResults = useApi(
    getNetAppFolders,
    [`folderPath=${folderPath}`],
    false,
  );

  useEffect(() => {
    netAppFolderApiResults.refetch();
  }, [folderPath]);

  useEffect(() => {
    if (location.pathname.endsWith("/netapp-connect")) {
      const operationName = searchParams.get("operation-name");
      if (!operationName) {
        return;
      }
      setFolderPath("");
    }
    return () => {
      window.history.replaceState({}, "");
    };
  }, []);

  useEffect(() => {
    if (netAppFolderApiResults.status === "failed")
      throw new Error(`${netAppFolderApiResults.error}`);
  }, [netAppFolderApiResults]);

  const handleConnectFolder = (id: string) => {
    setSelectedFolderId(id);
    navigate(`/case/${caseId}/netapp-connect/confirmation`);
  };
  const handleGetFolderContent = (folderPath: string) => {
    console.log("get netapp folders", folderPath);
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
