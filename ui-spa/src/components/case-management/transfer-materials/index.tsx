import { useEffect, useState, useMemo, useCallback, useRef } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { LinkButton, InsetText, NotificationBanner } from "../../govuk";
import NetAppFolderContainer from "./NetAppFolderContainer";
import { Spinner } from "../../common/Spinner";
import {
  getEgressFolders,
  getNetAppFolders,
  indexingFileTransfer,
  initiateFileTransfer,
  handleFileTransferClear,
} from "../../../apis/gateway-api";
import EgressFolderContainer from "./EgressFolderContainer";
import { useNavigate, useLocation } from "react-router-dom";
import TransferConfirmationModal from "./TransferConfirmationModal";
import { getGroupedFolderFileData } from "../../../common/utils/getGroupedFolderFileData";
import { TransferAction } from "../../../common/types/TransferAction";
import { getFormatedEgressFolderData } from "../../../common/utils/getFormatedEgressFolderData";
import { mapToNetAppFolderData } from "../../../common/utils/mapToNetAppFolderData";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import { InitiateFileTransferPayload } from "../../../common/types/InitiateFileTransferPayload";
import { IndexingFileTransferPayload } from "../../../common/types/IndexingFileTransferPayload";
import { TransferStatusResponse } from "../../../common/types/TransferStatusResponse";
import { IndexingFileTransferResponse } from "../../../common/types/IndexingFileTransferResponse";
import { InitiateFileTransferResponse } from "../../../common/types/InitiateFileTransferResponse";
import { useUserDetails } from "../../../auth";
import { ApiError } from "../../../common/errors/ApiError";
import { pollTransferStatus } from "../../../common/utils/pollTransferStatus";
import { getCommonPath } from "../../../common/utils/getCommonPath";
import styles from "./index.module.scss";

type TransferMaterialsPageProps = {
  isTabActive: boolean;
  caseId: string;
  operationName: string;
  egressWorkspaceId: string;
  netAppPath: string;
  activeTransferId: string | null;
};

const TransferMaterialsPage: React.FC<TransferMaterialsPageProps> = ({
  isTabActive,
  caseId,
  operationName,
  egressWorkspaceId,
  netAppPath,
  activeTransferId,
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { username } = useUserDetails();
  const [transferSource, setTransferSource] = useState<"egress" | "netapp">(
    "egress",
  );
  const [transferStatusData, setTransferStatusData] = useState<null | {
    username: string;
    direction: "EgressToNetApp" | "NetAppToEgress";
    transferType: "Move" | "Copy";
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
  const [apiRequestError, setApiRequestError] = useState<null | Error>(null);

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
          <h3 className={styles.inlineHeading}>Egress</h3>-{" "}
          <span>{transferSource === "egress" ? "Source" : "Destination"}</span>
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
          <h3 className={styles.inlineHeading}>Shared drive</h3>-{" "}
          <span>{transferSource === "egress" ? "Destination" : "Source"}</span>
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
          `/case/${caseId}/case-management/connection-error?type=shareddrive`,
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
    if (apiRequestError) {
      throw new Error(apiRequestError.message);
    }
  }, [apiRequestError]);

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

  const getIndexingFileTransferPayload = (): IndexingFileTransferPayload => {
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

    const payload = {
      caseId: parseInt(caseId),
      transferDirection:
        selectedTransferAction.destinationFolder.sourceType === "egress"
          ? ("EgressToNetApp" as const)
          : ("NetAppToEgress" as const),
      transferType:
        selectedTransferAction.actionType === "copy"
          ? ("Copy" as const)
          : ("Move" as const),
      sourcePaths: sourcePaths,
      destinationPath: selectedTransferAction.destinationFolder.path,
      workspaceId: egressWorkspaceId,
    };
    return payload;
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
      relativePath: data.relativePath,
      fullFilePath: data.fullFilePath,
    }));

    const relativePaths = sourcePaths.map((data) => data.relativePath!);

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
            sourceRootFolderPath: getCommonPath(relativePaths),
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
    const validationPayload = getIndexingFileTransferPayload();
    try {
      setTransferStatus("validating");
      response = await indexingFileTransfer(validationPayload);
      if (response.isInvalid) {
        setTransferStatus("validated-with-errors");
        navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`, {
          state: {
            isRouteValid: true,
            validationErrors: response.validationErrors,
            destinationPath: response.destinationPath,
            initiateTransferPayload: getInitiateTransferPayload(response),
            baseFolderName:
              "folderName" in currentEgressFolder
                ? currentEgressFolder.folderName
                : "",
          },
        });
        return;
      }

      const initiateTransferPayload = getInitiateTransferPayload(response);
      handleInitiateFileTransfer(initiateTransferPayload);
    } catch (error) {
      if (
        error instanceof ApiError &&
        error.code == 403 &&
        validationPayload.transferType === "Move"
      ) {
        navigate(`/case/${caseId}/case-management/transfer-permissions-error`, {
          state: {
            isRouteValid: true,
          },
        });
        return;
      }
      const newError =
        error instanceof ApiError
          ? error
          : new Error(
              `Invalid indexing file transfer api response. More details, ${error}`,
            );
      setApiRequestError(newError);
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
      transferType: initiatePayload.transferType,
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
      setApiRequestError(error as Error);
      return;
    }
  };

  const handleStatusResponse = useCallback(
    (response: TransferStatusResponse) => {
      if (response.status === "Initiated" || response.status === "InProgress") {
        setTransferStatus("transferring");
        setTransferStatusData({
          username: response.userName,
          direction: response.direction,
          transferType: response.transferType,
        });
        return;
      }
      if (response.status === "Completed") {
        setTransferStatus("completed");
        setTransferStatusData({
          username: response.userName,
          direction: response.direction,
          transferType: response.transferType,
        });
        if (response.userName === username)
          handleFileTransferClear(transferId!);

        setTransferId("");
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
          },
        });
        if (response.userName === username)
          handleFileTransferClear(transferId!);
        setTransferId("");
        setTransferStatusData(null);
      }
    },
    [caseId, navigate, setTransferStatus, setTransferId, username, transferId],
  );
  const isComponentUnmounted = useCallback(() => {
    return unMounting.current;
  }, []);

  useEffect(() => {
    unMounting.current = false;
    if (location.state) {
      window.history.replaceState({}, "", location.pathname + location.search);
    }
    return () => {
      unMounting.current = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
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

  const activeTransferMessage = useMemo(() => {
    if (!transferStatusData) {
      return {
        ariaLabelText: "",
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
  }, [transferStatusData, username]);

  if (!isTabActive) return <> </>;
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
                ? "Indexing transfer from Egress to Shared Drive"
                : "Indexing transfer from Shared Drive to Egress"
            }
          />
          <div className={styles.spinnerText}>
            {transferSource === "egress" ? (
              <span>
                Indexing transfer from <b>Egress to Shared Drive...</b>
              </span>
            ) : (
              <span>
                Indexing transfer from <b>Shared Drive to Egress...</b>
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
      {!transferId && (
        <div>
          <div className={styles.headerText}>
            <h2>{`${transferSource === "egress" ? "Transfer between Egress and Shared Drive" : "Transfer between Shared Drive and Egress"}`}</h2>
            <InsetText>
              <>
                Select the files or folders you want to transfer and where you
                want to put them.
                <br />
                You can also transfer
              </>
              <LinkButton onClick={handleSwitchSource}>
                {`${transferSource === "egress" ? "from the Shared Drive to Egress" : "from Egress to the Shared Drive"}`}
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
