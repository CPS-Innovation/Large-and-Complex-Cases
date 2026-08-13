import { useState, useEffect, useContext } from "react";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import { getConnectNetAppFolders } from "../../apis/gateway-api";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import { MainStateContext } from "../../providers/MainStateProvider";
import { useQuery } from "@tanstack/react-query";
import {
  useNavigate,
  useSearchParams,
  useLocation,
  useParams,
} from "react-router-dom";

const NetAppPage = () => {
  const { state, dispatch } = useContext(MainStateContext);
  const navigate = useNavigate();
  const { caseId } = useParams();
  const [searchParams] = useSearchParams();
  const location = useLocation();

  const { searchQueryString, netappRootFolderPath } =
    state.appData.connectSharedDrivePage;

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
    dispatch({
      type: "SET_SHARED_DRIVE_CONNECT_CONFIRMATION_PAGE",
      payload: {
        operationName: operationName,
        backLinkUrl: `/case/${caseId}/netapp-connect?${getUrlSearchParam("operation-name", operationName)}`,
        selectedWorkspace: {
          folderPath: path,
        },
      },
    });
    dispatch({
      type: "SET_SHARED_DRIVE_CONNECT_PAGE",
      payload: {
        netappRootFolderPath: rootFolderPath,
      },
    });
    navigate(`/case/${caseId}/netapp-connect/confirmation`);
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
