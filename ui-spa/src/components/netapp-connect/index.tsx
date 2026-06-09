import { useState, useEffect } from "react";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import { getConnectNetAppFolders } from "../../apis/gateway-api";
import { SharedDriveConnectRouteState } from "../../common/types/SharedDriveConnectRouteState";
import { SharedDriveConnectConfirmationRouteState } from "../../common/types/SharedDriveConnectConfirmationRouteState";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import { useQuery } from "@tanstack/react-query";
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

  const [operationName, setOperationName] = useState<string>("");
  const [rootFolderPath, setRootFolderPath] = useState(
    netappRootFolderPath ?? "",
  );

  const { data: netAppFolderResults, isLoading: isNetAppFolderResultsLoading } =
    useQuery({
      queryKey: [operationName, rootFolderPath],
      queryFn: () => getConnectNetAppFolders(operationName, rootFolderPath),
      retry: false,
      enabled: !!operationName,
      throwOnError: true,
      staleTime: 0,
      gcTime: 0,
    });

  useEffect(() => {
    if (location.pathname.endsWith("/netapp-connect")) {
      const opName = searchParams.get("operation-name");
      if (!opName) {
        setOperationName("");
        return;
      }
      setOperationName(opName);
    }
  }, [location, setOperationName, searchParams]);

  const handleGetFolderContent = (path: string) => {
    setRootFolderPath(path);
  };

  const handleConnectFolder = (path: string) => {
    const payload: SharedDriveConnectConfirmationRouteState = {
      isRouteValid: true,
      operationName: operationName,
      caseId: caseId!,
      searchQueryString: searchQueryString,
      netappRootFolderPath: rootFolderPath,
      backLinkUrl: `/case/${caseId}/netapp-connect?${getUrlSearchParam("operation-name", operationName)}`,
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
      netAppFolderResults={netAppFolderResults ?? { rootPath: "", folders: [] }}
      isNetAppFolderResultsLoading={isNetAppFolderResultsLoading}
      handleConnectFolder={handleConnectFolder}
      handleGetFolderContent={handleGetFolderContent}
    />
  );
};

export default NetAppPage;
