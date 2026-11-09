import { useState, useEffect, useContext } from "react";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import {
  getConnectNetAppFolders,
  connectNetAppFolder,
} from "../../apis/gateway-api";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import { MainStateContext } from "../../providers/MainStateProvider";
import { useQuery } from "@tanstack/react-query";
import {
  useNavigate,
  useSearchParams,
  useLocation,
  useParams,
} from "react-router";

const NetAppPage = () => {
  const { state, dispatch } = useContext(MainStateContext);
  const navigate = useNavigate();
  const { caseId: paramCaseId } = useParams();
  const [searchParams] = useSearchParams();
  const { caseId: caseRegistrationCaseId } =
    state.appData.caseRegistrationCaseDetails ?? {};
  const location = useLocation();

  const isRegistrationCreateFlow = location.pathname.includes(
    "/case-registration/shared-drive-create",
  );
  const isRegistrationConnectFlow = location.pathname.includes(
    "/case-registration/shared-drive-connect",
  );

  const isRegistrationFlow =
    isRegistrationCreateFlow || isRegistrationConnectFlow;

  const caseId = isRegistrationFlow ? caseRegistrationCaseId : paramCaseId;

  const {
    searchQueryString,
    netappRootFolderPath,
    accessibleRoots: persistedAccessibleRoots,
  } = state.appData.connectSharedDrivePage;

  const [operationName, setOperationName] = useState<string>("");
  const [rootFolderPath, setRootFolderPath] = useState(
    netappRootFolderPath ?? "",
  );

  // Only the root listing reports the restriction, so when we return from the confirmation page
  // mid-tree there is nothing to re-derive it from and we have to reuse the persisted value.
  const [accessibleRoots, setAccessibleRoots] = useState<string[] | null>(
    persistedAccessibleRoots ?? null,
  );

  console.log("operationName>>>", operationName);
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
    if (!netAppFolderResults || rootFolderPath !== "") {
      return;
    }
    setAccessibleRoots(
      netAppFolderResults.isRestrictedRoot
        ? (netAppFolderResults.accessibleRoots ?? [])
        : null,
    );
  }, [netAppFolderResults, rootFolderPath]);

  useEffect(() => {
    const opName = searchParams.get("operation-name");
    if (!opName) {
      setOperationName("");
      return;
    }
    setOperationName(opName);
  }, [location, setOperationName, searchParams]);

  const handleGetFolderContent = (path: string) => {
    setRootFolderPath(path);
  };

  const handleConnectFolder = async (path: string) => {
    if (isRegistrationConnectFlow) {
      dispatch({
        type: "SET_SHARED_DRIVE_CONNECT_PAGE",
        payload: {
          netappRootFolderPath: rootFolderPath,
          accessibleRoots,
        },
      });
    }

    if (isRegistrationCreateFlow) {
      dispatch({
        type: "SET_SELECTED_CREATE_SHARED_DRIVE_FOLDER_PATH",
        payload: path,
      });

      void navigate("/case-registration/shared-drive-templates");
      return;
    }

    try {
      if (caseId && operationName) {
        await connectNetAppFolder({
          operationName: operationName,
          folderPath: path,
          caseId: `${caseId}`,
        });
        if (isRegistrationFlow) {
          void navigate(
            "/case-registration/case-registration-final-confirmation",
          );
          return;
        }
        navigate(`/case/${caseId}/case-management`);
      }
    } catch (error) {
      if (error) {
        const basePath = isRegistrationFlow
          ? `/case-registration/shared-drive-connect`
          : `/case/${caseId}/shared-drive-connect`;
        dispatch({
          type: "SET_SHARED_DRIVE_CREATE_CONNECT_FAILURE_PAGE",
          payload: {
            backLinkUrl: `${basePath}?${getUrlSearchParam("operation-name", operationName)}`,
          },
        });
        if (isRegistrationFlow) {
          void navigate(`/case-registration/connect-shared-drive-folder-error`);
          return;
        }

        navigate(`/case/${caseId}/connect-shared-drive-folder-error`);
      }
    }
  };

  return (
    <NetAppFolderResultsPage
      backLinkUrl={`/search-results?${searchQueryString}`}
      rootFolderPath={rootFolderPath}
      netAppFolderResults={netAppFolderResults ?? { rootPath: "", folders: [] }}
      isNetAppFolderResultsLoading={isNetAppFolderResultsLoading}
      accessibleRoots={accessibleRoots}
      setupType={isRegistrationCreateFlow ? "create" : "connect"}
      handleConnectFolder={handleConnectFolder}
      handleGetFolderContent={handleGetFolderContent}
    />
  );
};

export default NetAppPage;
