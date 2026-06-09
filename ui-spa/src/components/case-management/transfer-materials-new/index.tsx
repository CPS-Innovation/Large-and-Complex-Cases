import { useEffect, useState, useMemo, useCallback, useRef } from "react";
import { NotificationBanner, Button } from "../../govuk";
import { Spinner } from "../../common/Spinner";
import {
  getEgressFolders,
  getNetAppFolders,
  handleFileTransferClear,
} from "../../../apis/gateway-api";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";

import { getFormattedEgressFolderData } from "../../../common/utils/getFormattedEgressFolderData";
import { mapToNetAppFolderData } from "../../../common/utils/mapToNetAppFolderData";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import type { TransferStatusResponse } from "../../../schemas";
import { useUserDetails } from "../../../auth";
import { ApiError } from "../../../common/errors/ApiError";
import { pollTransferStatus } from "../../../common/utils/pollTransferStatus";
import { useUserGroupsFeatureFlag } from "../../../common/hooks/useUserGroupsFeatureFlag";
import { getUrlSearchParam } from "../../../common/utils/getUrlSearchParam";
import TransferSourceNavigationTableContainer from "./TransferSourceNavigationTableContainer";
import {
  // type EgressFolderData,
  // type NetAppFolderData,
  type EgressFolder,
  type NetAppFileFolder,
} from "../../../schemas";
import TransferControls from "./TransferControls";
import FolderPath from "../../common/FolderPath";
import styles from "./index.module.scss";

type TransferMaterialsPageProps = {
  isTabActive: boolean;
  caseId: string;
  operationName: string;
  egressWorkspaceId: string;
  netAppPath: string;
  activeTransferId: string | null;
  urn: string;
};

const TransferMaterialsNewPage: React.FC<TransferMaterialsPageProps> = ({
  isTabActive,
  caseId,
  operationName,
  egressWorkspaceId,
  netAppPath,
  activeTransferId,
  urn,
}) => {
  const featureFlags = useUserGroupsFeatureFlag();
  const navigate = useNavigate();
  const { username } = useUserDetails();
  const [transferSource, setTransferSource] = useState<
    "egress" | "sharedDrive"
  >("egress");
  const [transferStatusData, setTransferStatusData] = useState<null | {
    username: string;
    direction: "EgressToNetApp" | "NetAppToEgress";
    transferType: "Move" | "Copy";
    transferMetrics: {
      totalFiles: number;
      processedFiles: number;
    } | null;
  }>(null);

  const [egressPathFolders, setEgressPathFolders] = useState<
    { folderName: string; folderPath: string; folderId: string }[]
  >([
    {
      folderName: "Home",
      folderPath: "Home/",
      folderId: "",
    },
  ]);

  const currentEgressFolder = useMemo(() => {
    if (egressPathFolders.length)
      return egressPathFolders[egressPathFolders.length - 1];
    return { folderId: "", folderPath: "", folderName: "" };
  }, [egressPathFolders]);

  const [netAppFolderPath, setNetAppFolderPath] = useState(netAppPath);

  const netAppPathFolders = useMemo(() => {
    const replacedString = netAppFolderPath.replace(netAppPath, "");
    const parts = replacedString.split("/").filter(Boolean);

    const result = parts.map((folderName, index) => ({
      folderName,
      folderPath: `${netAppPath}${parts.slice(0, index + 1).join("/")}/`,
    }));
    const withHome = [
      {
        folderName: getFolderNameFromPath(netAppPath),
        folderPath: netAppPath,
      },
      ...result,
    ];
    return withHome;
  }, [netAppFolderPath, netAppPath]);

  const [selectedSourceFoldersOrFiles, setSelectedSourceFoldersOrFiles] =
    useState<string[]>([]);

  const [transferStatus, setTransferStatus] = useState<
    | "validating"
    | "validated-with-errors"
    | "transferring"
    | "completed-with-errors"
    | "completed"
    | null
  >(null);
  const [transferId, setTransferId] = useState(activeTransferId);
  const [apiRequestError, setApiRequestError] = useState<null | Error>(null);

  const {
    data: egressData,
    refetch: egressRefetch,
    isLoading: isEgressFolderDataLoading,
    isError: isEgressError,
    error: egressError,
  } = useQuery({
    queryKey: [egressWorkspaceId, currentEgressFolder.folderId],
    queryFn: () =>
      getEgressFolders(egressWorkspaceId, currentEgressFolder.folderId),
    retry: false,
    enabled: !!egressWorkspaceId,
    staleTime: 0,
    gcTime: 0,
  });
  const egressFolderData = useMemo(
    () => (egressData ? getFormattedEgressFolderData(egressData) : []),
    [egressData],
  );

  const {
    data: netAppData,
    refetch: netAppRefetch,
    isLoading: isNetAppFolderDataLoading,
    isError: isNetAppError,
    error: netAppError,
  } = useQuery({
    queryKey: [netAppFolderPath],
    queryFn: () => getNetAppFolders(netAppFolderPath),
    retry: false,
    enabled: !!netAppFolderPath,
    throwOnError: true,
    staleTime: 0,
    gcTime: 0,
  });

  const netAppFolderData = useMemo(
    () => (netAppData ? mapToNetAppFolderData(netAppData) : []),
    [netAppData],
  );
  const unMounting = useRef(false);

  const handleEgressFolderPathClick = (path: string) => {
    const index = egressPathFolders.findIndex(
      (item) => item.folderPath === path,
    );
    const newData =
      index !== -1
        ? egressPathFolders.slice(0, index + 1)
        : [...egressPathFolders];
    setEgressPathFolders(newData);
    setSelectedSourceFoldersOrFiles([]);
  };

  const handleNetAppFolderPathClick = (path: string) => {
    setNetAppFolderPath(path);
    setSelectedSourceFoldersOrFiles([]);
  };

  const handleFolderPathClick = (path: string) => {
    if (transferSource === "egress") {
      handleEgressFolderPathClick(path);
    } else {
      handleNetAppFolderPathClick(path);
    }
  };

  const handleCheckboxChange = (checkboxId: string, checked: boolean) => {
    let updatedFolders: string[] = [];

    if (checkboxId === "all-folders") {
      if (checked) {
        if (transferSource === "egress")
          updatedFolders = [
            "all-folders",
            ...egressFolderData.map((data) => data.path),
          ];
        if (transferSource === "sharedDrive")
          updatedFolders = [
            "all-folders",
            ...netAppFolderData.map((data) => data.path),
          ];
      } else {
        updatedFolders = [];
      }
    } else if (!checked) {
      updatedFolders = selectedSourceFoldersOrFiles.filter(
        (item) => item !== checkboxId,
      );
    } else {
      updatedFolders = [...selectedSourceFoldersOrFiles, checkboxId];
    }

    setSelectedSourceFoldersOrFiles(updatedFolders);
  };

  const isSourceFolderChecked = (id: string) => {
    return selectedSourceFoldersOrFiles.includes(id);
  };

  const toggleTransferDirection = () => {
    setSelectedSourceFoldersOrFiles([]);
    if (transferSource === "egress") {
      setTransferSource("sharedDrive");
      return;
    }
    setTransferSource("egress");
  };

  useEffect(() => {
    console.log("egressPathFolders>>", egressPathFolders);
    if (!egressPathFolders.length && egressData?.[0]?.path)
      setEgressPathFolders([
        {
          folderName: getFolderNameFromPath(egressData[0].path),
          folderPath: egressData[0].path,
          folderId: "",
        },
      ]);
    if (!egressPathFolders.length && egressData)
      setEgressPathFolders([
        {
          folderName: "Home",
          folderPath: "Home/",
          folderId: "",
        },
      ]);
  }, [egressPathFolders, egressData, egressWorkspaceId]);

  useEffect(() => {
    if (isEgressError && egressError) {
      if ((egressError as ApiError).code === 404) {
        navigate(
          `/case/${caseId}/case-management/egress-connection-error?${getUrlSearchParam("operation-name", operationName)}`,
          {
            state: {
              isRouteValid: true,
            },
          },
        );
        return;
      }
      if ((egressError as ApiError).code === 401) {
        navigate(
          `/case/${caseId}/case-management/connection-error?type=egress`,
          {
            state: {
              isRouteValid: true,
            },
          },
        );
        return;
      } else {
        throw new Error(`${egressError}`);
      }
    } else if (isNetAppError && netAppError) {
      if ((netAppError as ApiError).code === 404) {
        navigate(
          `/case/${caseId}/case-management/shared-drive-connection-error?${getUrlSearchParam("operation-name", operationName)}`,
          {
            state: {
              isRouteValid: true,
            },
          },
        );
        return;
      }
      if ((netAppError as ApiError).code === 401) {
        navigate(
          `/case/${caseId}/case-management/connection-error?type=shareddrive`,
          {
            state: {
              isRouteValid: true,
            },
          },
        );
        return;
      } else {
        throw new Error(`${netAppError}`);
      }
    }
  }, [
    isEgressError,
    egressError,
    isNetAppError,
    netAppError,
    caseId,
    operationName,
    navigate,
  ]);

  useEffect(() => {
    if (apiRequestError) {
      throw apiRequestError;
    }
  }, [apiRequestError]);

  const getTransferSourcePath = ():
    | {
        fileId: string;
        path: string;
        isFolder: boolean;
      }[]
    | { path: string }[] => {
    if (transferSource === "egress") {
      return egressFolderData
        .filter((data) => selectedSourceFoldersOrFiles.includes(data.path))
        .map((data) => ({
          fileId: data.id,
          path: data.path,
          isFolder: data.isFolder,
        }));
    } else {
      return selectedSourceFoldersOrFiles.map((path) => ({
        path: path,
      }));
    }
  };

  const handleTransferAction = (type: "copy" | "move") => {
    // setSelectedTransferAction(type);
    navigate(`/case/${caseId}/case-management/transfer-destination-page`, {
      state: {
        isRouteValid: true,
        transferSource: transferSource,
        selectedTransferAction: type,
        sourcePaths: getTransferSourcePath(),
        egressWorkspaceId,
        caseId,
        netAppFolderPath,
        operationName,
      },
    });
  };

  const handleStatusResponse = useCallback(
    (response: TransferStatusResponse) => {
      if (response.status === "Initiated" || response.status === "InProgress") {
        setTransferStatus("transferring");
        setTransferStatusData({
          username: response.userName,
          direction: response.direction,
          transferType: response.transferType,
          transferMetrics: {
            totalFiles: response.totalFiles,
            processedFiles: response.processedFiles,
          },
        });
        return;
      }
      if (response.status === "Completed") {
        setTransferStatus("completed");
        setTransferStatusData({
          username: response.userName,
          direction: response.direction,
          transferType: response.transferType,
          transferMetrics: {
            totalFiles: response.totalFiles,
            processedFiles: response.processedFiles,
          },
        });
        if (response.userName === username)
          handleFileTransferClear(transferId!);

        setTransferId("");
        if (transferSource === "sharedDrive") {
          egressRefetch();
          return;
        }
        netAppRefetch();
        return;
      }
      if (
        response.status === "PartiallyCompleted" ||
        response.status === "Failed"
      ) {
        setTransferStatus("completed-with-errors");
        navigate(`/case/${caseId}/case-management/transfer-errors`, {
          state: {
            isRouteValid: true,
            transferId: transferId,
            failedItems: response.failedItems,
          },
        });
        if (response.userName === username)
          handleFileTransferClear(transferId!);
        setTransferId("");
        setTransferStatusData(null);
      }
    },
    [
      caseId,
      navigate,
      setTransferStatus,
      setTransferId,
      username,
      transferId,
      transferSource,
      egressRefetch,
      netAppRefetch,
    ],
  );
  const isComponentUnmounted = useCallback(() => {
    return unMounting.current;
  }, []);

  const handleDisconnectSharedDrive = async () => {
    navigate(
      `/case/${caseId}/case-management/disconnect-shared-drive-confirmation`,
      {
        state: {
          isRouteValid: true,
          caseId: caseId,
          urn: urn,
        },
      },
    );
  };

  useEffect(() => {
    unMounting.current = false;

    return () => {
      unMounting.current = true;
    };
  }, []);

  useEffect(() => {
    if (!transferId) {
      return;
    }
    setTransferStatus("transferring");
    pollTransferStatus(
      transferId,
      isComponentUnmounted,
      handleStatusResponse,
      setApiRequestError,
    );
  }, [
    transferId,
    setTransferStatus,
    isComponentUnmounted,
    handleStatusResponse,
  ]);

  const transferProgressMetrics = useMemo(() => {
    const defaultMetricsData = {
      progressAriaLiveText: "",
      progressContent: "",
    };
    if (!transferStatusData?.transferMetrics) return defaultMetricsData;

    if (!transferStatusData?.transferMetrics?.totalFiles) {
      return defaultMetricsData;
    }
    const progressAriaLiveText = `Transfer progress, ${transferStatusData.transferMetrics.processedFiles} out of ${transferStatusData.transferMetrics.totalFiles} files processed`;
    const progressContent = (
      <div
        className={styles.transferProgressMetrics}
        data-testid="transfer-progress-metrics"
      >
        <span>
          total files : {transferStatusData?.transferMetrics?.totalFiles}
        </span>
        <span>
          files processed :{" "}
          {transferStatusData?.transferMetrics?.processedFiles}
        </span>
      </div>
    );
    return {
      progressAriaLiveText,
      progressContent,
    };
  }, [transferStatusData]);

  const activeTransferMessage = useMemo(() => {
    if (transferStatus === "transferring") {
      if (!transferStatusData) {
        return {
          ariaLabelText: "Completing transfer",
          spinnerTextContent: <span> Completing transfer</span>,
        };
      }
      if (transferStatusData?.username !== username) {
        return {
          ariaLabelText: `${transferStatusData?.username} is currently transferring`,
          spinnerTextContent: (
            <span>
              <b>{transferStatusData?.username}</b> is currently transferring
            </span>
          ),
        };
      }

      if (transferStatusData?.direction === "EgressToNetApp")
        return {
          ariaLabelText: "Completing transfer from Egress to Shared Drive",
          spinnerTextContent: (
            <span>
              Completing transfer from <b>Egress to Shared Drive...</b>
            </span>
          ),
        };
      return {
        ariaLabelText: "Completing transfer from Shared Drive to Egress",
        spinnerTextContent: (
          <span>
            Completing transfer from <b>Shared Drive to Egress...</b>
          </span>
        ),
      };
    }
    if (transferStatus === "validating") {
      if (transferSource === "egress") {
        return {
          ariaLabelText: "Indexing transfer from egress to shared drive",
          spinnerTextContent: (
            <span>
              Indexing transfer from <b>Egress to Shared Drive...</b>
            </span>
          ),
        };
      }
      return {
        ariaLabelText: "Indexing transfer from shared drive to egress",
        spinnerTextContent: (
          <span>
            Indexing transfer from <b>Shared Drive to Egress...</b>
          </span>
        ),
      };
    }
  }, [transferStatusData, username, transferStatus, transferSource]);

  const handleFolderClick = (data: EgressFolder | NetAppFileFolder) => {
    if (transferSource === "egress" && "id" in data) {
      console.log("Folder clicked:", data);
      // TypeScript now narrows `data` to EgressFolder within this block
      setEgressPathFolders((prevItems) => [
        ...prevItems,
        {
          folderId: data.id,
          folderPath: data.path,
          folderName: data.name,
        },
      ]);
      if (transferSource === "egress") setSelectedSourceFoldersOrFiles([]);
    }
    if (transferSource === "sharedDrive") {
      console.log("NetApp folder clicked:", data);
      setNetAppFolderPath(data.path);
      if (transferSource === "sharedDrive") setSelectedSourceFoldersOrFiles([]);
    }
  };

  const handleTableSort = (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => {
    console.log("Table sorted:", { sortName, sortType });
  };

  const getMainTexts = useCallback(() => {
    if (transferSource === "egress") {
      return {
        title: "Transfer from Egress to the Shared Drive",
        description:
          "Select the files or folders you want to transfer. Then choose where to save them on the Shared Drive.",
      };
    }
    return {
      title: "Transfer from Shared Drive to Egress",
      description:
        "Select the files or folders you want to transfer. Then choose where to save them on Egress.",
    };
  }, [transferSource]);
  if (!isTabActive) return <> </>;

  return (
    <div>
      <output aria-live="polite" className="govuk-visually-hidden">
        {activeTransferMessage?.ariaLabelText}
        {transferProgressMetrics.progressAriaLiveText && (
          <>{transferProgressMetrics.progressAriaLiveText}</>
        )}
      </output>
      {transferStatus === "validating" && (
        <div className={styles.transferContent}>
          <div className={styles.spinnerWrapper}>
            <Spinner data-testid="transfer-spinner" diameterPx={50} />
            <div className={styles.spinnerText}>
              {activeTransferMessage?.spinnerTextContent}
            </div>
          </div>
        </div>
      )}
      {transferStatus === "transferring" && (
        <div className={styles.transferContent}>
          <div className={styles.spinnerWrapper}>
            <Spinner data-testid="transfer-spinner" diameterPx={50} />
            <div className={styles.spinnerText}>
              {activeTransferMessage?.spinnerTextContent}
              {transferProgressMetrics.progressContent && (
                <>{transferProgressMetrics.progressContent}</>
              )}
            </div>
          </div>
        </div>
      )}

      {transferStatus === "completed" &&
        transferStatusData?.username === username && (
          <div className={styles.successBanner}>
            <NotificationBanner
              type="success"
              data-testid="transfer-success-notification-banner"
            >
              <b className={styles.successMessage}>
                Files{" "}
                {transferStatusData.transferType === "Copy"
                  ? "copied"
                  : "moved"}{" "}
                successfully
              </b>
            </NotificationBanner>
          </div>
        )}
      {!transferId &&
        transferStatus !== "validating" &&
        transferStatus !== "transferring" && (
          <div>
            <div className={styles.headerText}>
              <h2>{getMainTexts().title}</h2>
              <div className={styles.insetTextWrapper}>
                <p>{getMainTexts().description}</p>
                <FolderPath
                  folders={
                    transferSource === "egress"
                      ? egressPathFolders
                      : netAppPathFolders
                  }
                  disabled={
                    transferSource === "egress"
                      ? isEgressFolderDataLoading
                      : isNetAppFolderDataLoading
                  }
                  handleFolderPathClick={handleFolderPathClick}
                />
                <TransferControls
                  transferSource={transferSource}
                  toggleTransferDirection={toggleTransferDirection}
                  disableControls={!selectedSourceFoldersOrFiles.length}
                  onCopy={() => handleTransferAction("copy")}
                  onMove={() => handleTransferAction("move")}
                />
                {featureFlags.disconnectSharedDrive && (
                  <Button
                    className={`govuk-button--secondary ${styles.disconnectButton}`}
                    name="secondary"
                    onClick={handleDisconnectSharedDrive}
                  >
                    Disconnect Shared Drive
                  </Button>
                )}
              </div>
            </div>
            <div
              className={styles.mainContainer}
              data-testid="transfer-main-container"
            >
              <TransferSourceNavigationTableContainer
                folderData={
                  transferSource === "egress"
                    ? { type: "egress", data: egressFolderData }
                    : { type: "netapp", data: netAppFolderData }
                }
                isLoading={
                  isEgressFolderDataLoading || isNetAppFolderDataLoading
                }
                handleFolderClick={handleFolderClick}
                handleTableSort={handleTableSort}
                handleCheckboxChange={handleCheckboxChange}
                isSourceFolderChecked={isSourceFolderChecked}
              />
            </div>
            <TransferControls
              transferSource={transferSource}
              toggleTransferDirection={toggleTransferDirection}
              disableControls={!selectedSourceFoldersOrFiles.length}
              onCopy={() => handleTransferAction("copy")}
              onMove={() => handleTransferAction("move")}
            />
          </div>
        )}
    </div>
  );
};

export default TransferMaterialsNewPage;
