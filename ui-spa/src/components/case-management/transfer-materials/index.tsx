import { useEffect, useState, useMemo, useCallback } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { LinkButton, InsetText, NotificationBanner } from "../../govuk";
import NetAppFolderContainer from "./NetAppFolderContainer";
import { Spinner } from "../../common/Spinner";
import {
  getEgressFolders,
  getNetAppFolders,
  validateFileTransfer,
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
import { ValidateFileTransferResponse } from "../../../common/types/ValidateFileTransferResponse";
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
  const [transferSource, setTransferSource] = useState<"egress" | "netapp">(
    "egress",
  );
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
          id: data.id,
          path: data.path,
        }));
    } else {
      sourcePaths = selectedSourceFoldersOrFiles.map((path) => ({
        path: path,
      }));
    }

    const validationPayload = {
      caseId: caseId,
      transferType:
        selectedTransferAction.actionType === "copy"
          ? ("COPY" as const)
          : ("MOVE" as const),
      direction:
        selectedTransferAction.destinationFolder.sourceType === "egress"
          ? ("EgressToNetApp" as const)
          : ("NetAppToEgress" as const),
      sourcePaths: sourcePaths,
      destinationBasePath: selectedTransferAction.destinationFolder.path,
    };
    return validationPayload;
  };

  const getInitiateTransferPayload = (
    isRetry: boolean,
    response: ValidateFileTransferResponse,
  ): InitiateFileTransferPayload => {
    if (!selectedTransferAction) {
      throw new Error(
        "selected transfer destination details should not be null",
      );
    }
    const sourcePaths = response.discoveredFiles.map((data) => ({
      id: data?.id,
      path: data.sourcePath,
    }));

    const payload = {
      isRetry: isRetry,
      caseId: caseId,
      sourcePaths: sourcePaths,
      destinationPath: response.destinationBasePath,
    };
    const uniquePayload =
      selectedTransferAction.destinationFolder.sourceType === "egress"
        ? {
            transferType:
              selectedTransferAction.actionType === "copy"
                ? ("COPY" as const)
                : ("MOVE" as const),
            direction: "EgressToNetApp" as const,
          }
        : {
            transferType: "COPY" as const,
            direction: "NetAppToEgress" as const,
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
    try {
      const validationPayload = getValidateTransferPayload();
      setTransferStatus("validating");
      const response = await validateFileTransfer(validationPayload);
      if (!response.isValid) {
        setTransferStatus("validated-with-errors");
        navigate(`/case/${caseId}/case-management/transfer-validation-errors`, {
          state: {
            isValid: true,
          },
        });
        return;
      }

      const initiateTransferPayload = getInitiateTransferPayload(
        false,
        response,
      );
      handleInitiateFileTransfer(initiateTransferPayload);
    } catch (error) {
      console.log(error);
    }
  };

  const handleInitiateFileTransfer = async (
    initiatePayload: InitiateFileTransferPayload,
  ) => {
    setTransferStatus("transferring");
    try {
      const initiateFileTransferResponse =
        await initiateFileTransfer(initiatePayload);
      if (initiateFileTransferResponse.transferId) {
        setTransferId(initiateFileTransferResponse.transferId);
      }
    } catch (e) {
      console.log(e);
    }
  };

  const handleStatusResponse = useCallback(
    (status: TransferStatusResponse, interval: NodeJS.Timeout) => {
      if (status.overallStatus === "COMPLETED") {
        egressRefetch();
        netAppRefetch();
        setTransferStatus("completed");
        setTransferId("");
        if (interval) clearInterval(interval);
        return;
      }
      if (status.overallStatus === "PARTIALLY_COMPLETED") {
        setTransferStatus("completed-with-errors");
        navigate(`/case/${caseId}/case-management/transfer-errors`, {
          state: {
            isValid: true,
          },
        });
        setTransferId("");
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
      if (transferId) {
        const status = await getTransferStatus(transferId);
        handleStatusResponse(status, interval);
      }
    };

    fetchStatusData();
    const interval = setInterval(async () => {
      fetchStatusData();
    }, pollingInterval);
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [transferId, setTransferStatus, handleStatusResponse]);

  if (transferStatus === "transferring") {
    return (
      <div className={styles.transferContent}>
        <div className={styles.spinnerWrapper}>
          <Spinner
            data-testid="transfer-spinner"
            diameterPx={50}
            ariaLabel={"Completing transfer from egress to shared drive"}
          />
          <div className={styles.spinnerText}>
            Completing transfer from <b>egress to shared drive...</b>
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
            ariaLabel={"Indexing transfer from egress to shared drive"}
          />
          <div className={styles.spinnerText}>
            Indexing transfer from <b>egress to shared drive...</b>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      {transferStatus === "completed" && (
        <div className={styles.successBanner}>
          <NotificationBanner type="success">
            Files copied successfully
          </NotificationBanner>
        </div>
      )}
      <div className={styles.headerText}>
        <h2>{`${transferSource === "egress" ? "Transfer folders and files between egress and shared drive" : "Transfer folders and files between shared drive and egress"}`}</h2>
        <InsetText>
          Select the folders and files you want to transfer, then choose a
          destination. You can switch the source and destination if needed.{" "}
          <LinkButton onClick={handleSwitchSource}> Switch source</LinkButton>
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
