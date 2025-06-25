import { useEffect, useState, useMemo, useCallback } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { LinkButton, InsetText, NotificationBanner } from "../../govuk";
import NetAppFolderContainer from "./NetAppFolderContainer";
import { Spinner } from "../../common/Spinner";
import {
  getEgressFolders,
  getNetAppFolders,
  indexingFileTransfer,
  initiateFileTransfer,
  getTransferStatus,
} from "../../../apis/gateway-api";
import EgressFolderContainer from "./EgressFolderContainer";
import { useNavigate } from "react-router-dom";
import TransferConfirmationModal from "./TransferConfirmationModal";
import { getGroupedFolderFileData } from "../../../common/utils/getGroupedFolderFileData";
import { TransferAction } from "../../../common/types/TransferAction";
import { getFormatedEgressFolderData } from "../../../common/utils/getFormatedEgressFolderData";
import { mapToNetAppFolderData } from "../../../common/utils/mapToNetAppFolderData";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import { InitiateFileTransferPayload } from "../../../common/types/InitiateFileTransferPayload";
import { TransferStatusResponse } from "../../../common/types/TransferStatusResponse";
import { IndexingFileTransferResponse } from "../../../common/types/IndexingFileTransferResponse";
import { InitiateFileTransferResponse } from "../../../common/types/InitiateFileTransferResponse";
import { useUserDetails } from "../../../auth";
import { ApiError } from "../../../common/errors/ApiError";
import styles from "./index.module.scss";

type TransferMaterialsPageProps = {
  caseId: string;
  operationName: string;
  egressWorkspaceId: string;
  netAppPath: string;
  activeTransferId: string | null;
};

const TransferMaterialsPage: React.FC<TransferMaterialsPageProps> = ({
  caseId,
  operationName,
  egressWorkspaceId,
  netAppPath,
  activeTransferId,
}) => {
  const navigate = useNavigate();
  const { username } = useUserDetails();
  const [transferSource, setTransferSource] = useState<"egress" | "netapp">(
    "egress",
  );
  const [transferStatusData, setTransferStatusData] = useState<null | {
    username: string;
    direction: "EgressToNetApp" | "NetAppToEgress";
  }>(null);
  const [egressPathFolders, setEgressPathFolders] = useState<
    {
      folderName: string;
      folderPath: string;
      folderId: string;
    }[]
  >([]);
  const [netAppFolderPath, setNetAppFolderPath] = useState(netAppPath);
  const [selectedSourceFoldersOrFiles, setSelectedSourceFoldersOrFiles] =
    useState<string[]>([]);
  const [selectedTransferAction, setSelectedTransferAction] =
    useState<TransferAction | null>(null);
  const [showTransferConfirmationModal, setShowTransferConfirmationModal] =
    useState<boolean>(false);

  const [transferStatus, setTransferStatus] = useState<
    | "validating"
    | "validated-with-errors"
    | "transferring"
    | "completed-with-errors"
    | "completed"
    | null
  >(null);
  const [transferId, setTransferId] = useState(activeTransferId);
  const [postRequestApiError, setPostRequestApiError] = useState<null | Error>(
    null,
  );

  const currentEgressFolder = useMemo(() => {
    if (egressPathFolders.length)
      return egressPathFolders[egressPathFolders.length - 1];
    return { folderId: "" };
  }, [egressPathFolders]);

  const {
    refetch: egressRefetch,
    status: egressStatus,
    data: egressData,
    error: egressError,
  } = useApi(
    getEgressFolders,
    [egressWorkspaceId, currentEgressFolder.folderId],
    false,
  );

  const {
    refetch: netAppRefetch,
    status: netAppStatus,
    data: netAppData,
    error: netAppError,
  } = useApi(getNetAppFolders, [netAppFolderPath], false);
  const egressFolderData = useMemo(
    () => (egressData ? getFormatedEgressFolderData(egressData) : []),
    [egressData],
  );

  const netAppFolderData = useMemo(
    () => (netAppData ? mapToNetAppFolderData(netAppData) : []),
    [netAppData],
  );

  const handleEgressFolderPathClick = (path: string) => {
    const index = egressPathFolders.findIndex(
      (item) => item.folderPath === path,
    );
    const newData =
      index !== -1
        ? egressPathFolders.slice(0, index + 1)
        : [...egressPathFolders];
    setEgressPathFolders(newData);
    if (transferSource === "egress") setSelectedSourceFoldersOrFiles([]);
  };

  const handleEgressFolderClick = (id: string) => {
    const folderData = egressFolderData.find((item) => item.id === id);
    if (folderData)
      setEgressPathFolders((prevItems) => [
        ...prevItems,
        {
          folderId: folderData.id,
          folderPath: folderData.path,
          folderName: folderData.name,
        },
      ]);
    if (transferSource === "egress") setSelectedSourceFoldersOrFiles([]);
  };

  const handleNetAppFolderClick = (path: string) => {
    setNetAppFolderPath(path);
    if (transferSource === "netapp") setSelectedSourceFoldersOrFiles([]);
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
        if (transferSource === "netapp")
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

  const handleSwitchSource = () => {
    setSelectedSourceFoldersOrFiles([]);
    if (transferSource === "egress") {
      setTransferSource("netapp");
      return;
    }
    setTransferSource("egress");
  };

  const renderEgressContainer = () => {
    return (
      <div
        className={
          transferSource === "egress"
            ? styles.sourceContainer
            : styles.destinationContainer
        }
        data-testid="egress-container"
      >
        <div className={styles.titleWrapper}>
          <h3>Egress Inbound documents</h3>
        </div>
        <div className={styles.tableContainer}>
          {
            <EgressFolderContainer
              transferSource={transferSource}
              egressFolderData={egressFolderData}
              egressDataStatus={egressStatus}
              egressPathFolders={egressPathFolders}
              selectedSourceLength={selectedSourceFoldersOrFiles.length}
              handleFolderPathClick={handleEgressFolderPathClick}
              handleFolderClick={handleEgressFolderClick}
              handleCheckboxChange={handleCheckboxChange}
              isSourceFolderChecked={isSourceFolderChecked}
              handleSelectedActionType={handleSelectedActionType}
            />
          }
        </div>
      </div>
    );
  };

  const handleSelectedActionType = (transferAction: TransferAction) => {
    setShowTransferConfirmationModal(true);
    setSelectedTransferAction(transferAction);
  };

  const handleCloseTransferConfirmationModal = () => {
    setShowTransferConfirmationModal(false);
  };

  const renderNetappContainer = () => {
    return (
      <div
        className={
          transferSource === "netapp"
            ? styles.sourceContainer
            : styles.destinationContainer
        }
        data-testid="netapp-container"
      >
        <div className={styles.titleWrapper}>
          <h3>Shared drive</h3>
        </div>
        <div className={styles.tableContainer}>
          {netAppPath && (
            <NetAppFolderContainer
              transferSource={transferSource}
              connectedFolderPath={netAppPath}
              currentFolderPath={netAppFolderPath}
              netAppFolderDataStatus={netAppStatus}
              netAppFolderData={netAppFolderData}
              selectedSourceLength={selectedSourceFoldersOrFiles.length}
              handleGetFolderContent={handleNetAppFolderClick}
              handleCheckboxChange={handleCheckboxChange}
              isSourceFolderChecked={isSourceFolderChecked}
              handleSelectedActionType={handleSelectedActionType}
            />
          )}
        </div>
      </div>
    );
  };

  useEffect(() => {
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
    if (egressStatus === "failed" && egressError) {
      if (egressError.code === 404) {
        navigate(
          `/case/${caseId}/case-management/egress-connection-error?operation-name=${operationName}`,
          {
            state: {
              isValid: true,
            },
          },
        );
        return;
      }
      if (egressError.code === 401) {
        navigate(
          `/case/${caseId}/case-management/connection-error?type=egress`,
        );
        return;
      } else {
        throw new Error(`${egressError}`);
      }
    } else if (netAppStatus === "failed" && netAppError) {
      if (netAppError.code === 404) {
        navigate(
          `/case/${caseId}/case-management/shared-drive-connection-error?operation-name=${operationName}`,
          {
            state: {
              isValid: true,
            },
          },
        );
        return;
      }
      if (netAppError.code === 401) {
        navigate(
          `/case/${caseId}/case-management/connection-error?type=shared drive`,
        );
        return;
      } else {
        throw new Error(`${netAppError}`);
      }
    }
  }, [
    egressStatus,
    egressError,
    netAppStatus,
    netAppError,
    caseId,
    operationName,
    navigate,
  ]);

  useEffect(() => {
    if (postRequestApiError) {
      throw new Error(postRequestApiError.message);
    }
  }, [postRequestApiError]);

  useEffect(() => {
    if (egressWorkspaceId && !transferId) {
      egressRefetch();
    }
  }, [egressWorkspaceId, egressRefetch, transferId]);

  useEffect(() => {
    if (netAppFolderPath && !transferId) {
      netAppRefetch();
    }
  }, [netAppFolderPath, netAppRefetch, transferId]);

  const getValidateTransferPayload = () => {
    if (!selectedTransferAction) {
      throw new Error(
        "selected transfer destination details should not be null",
      );
    }
    let sourcePaths = [];
    if (selectedTransferAction.destinationFolder.sourceType === "egress") {
      sourcePaths = egressFolderData
        .filter((data) => selectedSourceFoldersOrFiles.includes(data.path))
        .map((data) => ({
          fileId: data.id,
          path: data.path,
          isFolder: data.isFolder,
        }));
    } else {
      sourcePaths = selectedSourceFoldersOrFiles.map((path) => ({
        path: path,
      }));
    }

    const validationPayload = {
      caseId: parseInt(caseId),
      transferDirection:
        selectedTransferAction.destinationFolder.sourceType === "egress"
          ? ("EgressToNetApp" as const)
          : ("NetAppToEgress" as const),
      sourcePaths: sourcePaths,
      destinationPath: selectedTransferAction.destinationFolder.path,
      workspaceId: egressWorkspaceId,
    };
    return validationPayload;
  };

  const getInitiateTransferPayload = (
    response: IndexingFileTransferResponse,
  ): InitiateFileTransferPayload => {
    if (!selectedTransferAction) {
      throw new Error(
        "selected transfer destination details should not be null",
      );
    }
    const sourcePaths = response.files.map((data) => ({
      fileId: data?.id,
      path: data.sourcePath,
    }));

    const payload = {
      caseId: parseInt(caseId),
      sourcePaths: sourcePaths,
      destinationPath: response.destinationPath,
      workspaceId: egressWorkspaceId,
    };
    const uniquePayload =
      selectedTransferAction.destinationFolder.sourceType === "egress"
        ? {
            transferType:
              selectedTransferAction.actionType === "copy"
                ? ("Copy" as const)
                : ("Move" as const),
            transferDirection: "EgressToNetApp" as const,
          }
        : {
            transferType: "Copy" as const,
            transferDirection: "NetAppToEgress" as const,
          };

    return { ...payload, ...uniquePayload };
  };

  const handleTransferConfirmationContinue = () => {
    handleValidateTransfer();
  };

  const handleValidateTransfer = async () => {
    setShowTransferConfirmationModal(false);
    setSelectedSourceFoldersOrFiles([]);
    setSelectedTransferAction(null);

    let response: IndexingFileTransferResponse;
    try {
      const validationPayload = getValidateTransferPayload();
      setTransferStatus("validating");
      response = await indexingFileTransfer(validationPayload);
      if (response.isInvalid) {
        setTransferStatus("validated-with-errors");
        navigate(`/case/${caseId}/case-management/transfer-validation-errors`, {
          state: {
            isRouteValid: true,
          },
        });
        return;
      }

      const initiateTransferPayload = getInitiateTransferPayload(response);
      handleInitiateFileTransfer(initiateTransferPayload);
    } catch (error) {
      const newError =
        error instanceof ApiError
          ? error
          : new Error(
              `Invalid indexing file transfer api response. More details, ${error}`,
            );
      setPostRequestApiError(newError);
      return;
    }
  };

  const handleInitiateFileTransfer = async (
    initiatePayload: InitiateFileTransferPayload,
  ) => {
    setTransferStatus("transferring");
    setTransferStatusData({
      username: username,
      direction:
        transferSource === "egress" ? "EgressToNetApp" : "NetAppToEgress",
    });
    let initiateFileTransferResponse: InitiateFileTransferResponse;
    try {
      initiateFileTransferResponse =
        await initiateFileTransfer(initiatePayload);

      if (!initiateFileTransferResponse.id) {
        throw new Error(
          "Invalid initiate transfer response, id does not exist",
        );
      }
      setTransferId(initiateFileTransferResponse.id);
    } catch (error) {
      setPostRequestApiError(error as Error);
      return;
    }
  };

  const handleStatusResponse = useCallback(
    (response: TransferStatusResponse, interval: NodeJS.Timeout) => {
      if (response.status === "Initiated" || response.status === "InProgress") {
        setTransferStatus("transferring");
        setTransferStatusData({
          username: response.userName,
          direction: response.direction,
        });
        return;
      }
      if (response.status === "Completed") {
        egressRefetch();
        netAppRefetch();
        setTransferStatus("completed");
        setTransferId("");
        if (interval) clearInterval(interval);
        return;
      }
      if (response.status === "PartiallyCompleted") {
        setTransferStatus("completed-with-errors");
        navigate(`/case/${caseId}/case-management/transfer-errors`, {
          state: {
            isRouteValid: true,
          },
        });
        setTransferId("");
        setTransferStatusData(null);
        if (interval) clearInterval(interval);
      }
    },
    [
      caseId,
      navigate,
      egressRefetch,
      netAppRefetch,
      setTransferStatus,
      setTransferId,
    ],
  );

  useEffect(() => {
    if (!transferId) {
      return;
    }
    setTransferStatus("transferring");

    const pollingInterval = 5000;
    const fetchStatusData = async () => {
      const status = await getTransferStatus(transferId);
      handleStatusResponse(status, interval);
    };

    fetchStatusData();
    const interval = setInterval(async () => {
      fetchStatusData();
    }, pollingInterval);
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [transferId, setTransferStatus, handleStatusResponse]);

  const activeTransferMessage = useMemo(() => {
    if (!transferStatusData) {
      return {
        ariaLabelText: "",
        spinnerTextContent: "",
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
        ariaLabelText: "Completing transfer from egress to shared drive",
        spinnerTextContent: (
          <span>
            Completing transfer from <b>egress to shared drive...</b>
          </span>
        ),
      };
    return {
      ariaLabelText: "Completing transfer from shared drive to egress",
      spinnerTextContent: (
        <span>
          Completing transfer from <b>shared drive to egress...</b>
        </span>
      ),
    };
  }, [transferStatusData, username]);

  if (transferStatus === "transferring") {
    return (
      <div className={styles.transferContent}>
        <div className={styles.spinnerWrapper}>
          <Spinner
            data-testid="transfer-spinner"
            diameterPx={50}
            ariaLabel={activeTransferMessage.ariaLabelText}
          />
          <div className={styles.spinnerText}>
            {activeTransferMessage.spinnerTextContent}
          </div>
        </div>
      </div>
    );
  }

  if (transferStatus === "validating") {
    return (
      <div className={styles.transferContent}>
        <div className={styles.spinnerWrapper}>
          <Spinner
            data-testid="transfer-spinner"
            diameterPx={50}
            ariaLabel={
              transferSource === "egress"
                ? "Indexing transfer from egress to shared drive"
                : "Indexing transfer from shared drive to egress"
            }
          />
          <div className={styles.spinnerText}>
            {transferSource === "egress" ? (
              <span>
                Indexing transfer from <b>egress to shared drive...</b>
              </span>
            ) : (
              <span>
                Indexing transfer from <b>shared drive to egress...</b>
              </span>
            )}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      {transferStatus === "completed" &&
        transferStatusData?.username === username && (
          <div className={styles.successBanner}>
            <NotificationBanner
              type="success"
              data-testid="transfer-success-notification-banner"
            >
              Files copied successfully
            </NotificationBanner>
          </div>
        )}
      {!transferId && (
        <div>
          <div className={styles.headerText}>
            <h2>{`${transferSource === "egress" ? "Transfer folders and files between egress and shared drive" : "Transfer folders and files between shared drive and egress"}`}</h2>
            <InsetText>
              Select the folders and files you want to transfer, then choose a
              destination. You can switch the source and destination if needed.{" "}
              <LinkButton onClick={handleSwitchSource}>
                {" "}
                Switch source
              </LinkButton>
            </InsetText>
          </div>
          <div
            className={styles.mainContainer}
            data-testid="transfer-main-container"
          >
            {transferSource === "egress" ? (
              <>
                {renderEgressContainer()}
                {renderNetappContainer()}
              </>
            ) : (
              <>
                {renderNetappContainer()}
                {renderEgressContainer()}
              </>
            )}
          </div>
        </div>
      )}
      {showTransferConfirmationModal && selectedTransferAction && (
        <TransferConfirmationModal
          transferAction={selectedTransferAction}
          groupedData={
            transferSource === "egress"
              ? getGroupedFolderFileData(
                  selectedSourceFoldersOrFiles,
                  egressFolderData,
                )
              : getGroupedFolderFileData(
                  selectedSourceFoldersOrFiles,
                  netAppFolderData,
                )
          }
          handleCloseModal={handleCloseTransferConfirmationModal}
          handleContinue={handleTransferConfirmationContinue}
        />
      )}
    </div>
  );
};

export default TransferMaterialsPage;
