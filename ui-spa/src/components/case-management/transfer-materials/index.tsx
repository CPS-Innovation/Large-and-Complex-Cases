import { useEffect, useState, useMemo } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { LinkButton, InsetText, Checkboxes, Button } from "../../govuk";
import NetAppFolderContainer from "./NetAppFolderContainer";
import { getEgressFolders, getNetAppFolders } from "../../../apis/gateway-api";
import EgressFolderContainer from "./EgressFolderContainer";
import { Modal } from "../../common/Modal";
import { useNavigate } from "react-router-dom";
import styles from "./index.module.scss";

type TransferMaterialsPageProps = {
  caseId: string | undefined;
  operationName: string | undefined;
  egressWorkspaceId: string | undefined;
  netAppPath: string | undefined;
};

const TransferMaterialsPage: React.FC<TransferMaterialsPageProps> = ({
  caseId,
  operationName,
  egressWorkspaceId,
  netAppPath,
}) => {
  const navigate = useNavigate();
  const [transferSource, setTransferSource] = useState<"egress" | "netapp">(
    "egress",
  );
  const [egressPathFolders, setEgressPathFolders] = useState<
    {
      folderName: string;
      folderPath: string;
      folderId?: string;
    }[]
  >([{ folderName: "Home", folderPath: "", folderId: "" }]);
  const [netAppFolderPath, setNetAppFolderPath] = useState("");
  const [selectedSourceFoldersOrFiles, setSelectedSourceFoldersOrFiles] =
    useState<string[]>([]);
  const [selectedTransferAction, setSelectedTransferAction] = useState<{
    destinationFolder:
      | {
          path: string;
          name: string;
          type: "netapp";
        }
      | {
          id: string;
          name: string;
          type: "egress";
        };
    actionType: "move" | "copy";
  } | null>(null);

  const [showTransferConfirmationModal, setShowTransferConfrimationModal] =
    useState<boolean>(false);

  useEffect(() => {
    if (netAppPath) setNetAppFolderPath(netAppPath);
  }, [netAppPath]);

  const currentEgressFolder = useMemo(() => {
    return egressPathFolders[egressPathFolders.length - 1];
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

  const handleEgressFolderClick = (id: string) => {
    const folderData = egressData!.find((item) => item.id === id);
    if (folderData)
      setEgressPathFolders((prevItems) => [
        ...prevItems,
        {
          folderId: folderData.id,
          folderPath: `${folderData.path}/${folderData.name}`,
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
            ...egressData!.map((data) => data.id),
          ];
        if (transferSource === "netapp")
          updatedFolders = [
            "all-folders",
            ...[...netAppData!.folderData, ...netAppData!.fileData].map(
              (data) => data.path,
            ),
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
        data-testId="egress-container"
      >
        <div className={styles.titleWrapper}>
          <h3>Egress Inbound documents</h3>
        </div>
        <div className={styles.tableContainer}>
          {
            <EgressFolderContainer
              transferSource={transferSource}
              egressData={egressData}
              egressDataStatus={egressStatus}
              egressPathFolders={egressPathFolders}
              selectedSourceLength={selectedSourceFoldersOrFiles.length}
              handleFolderPathClick={handleEgressFolderPathClick}
              handleFolderClick={handleEgressFolderClick}
              handleCheckboxChange={handleCheckboxChange}
              isSourceFolderChecked={isSourceFolderChecked}
            />
          }
        </div>
      </div>
    );
  };

  const handleSelectedActionType = (transferAction: {
    destinationFolder:
      | {
          path: string;
          name: string;
          type: "netapp";
        }
      | {
          id: string;
          name: string;
          type: "egress";
        };
    actionType: "move" | "copy";
  }) => {
    setShowTransferConfrimationModal(true);
    setSelectedTransferAction(transferAction);
  };

  const handleCloseTransferConfirmationModal = () => {
    setShowTransferConfrimationModal(false);
  };

  const renderNetappContainer = () => {
    return (
      <div
        className={
          transferSource === "netapp"
            ? styles.sourceContainer
            : styles.destinationContainer
        }
        data-testId="netapp-container"
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
              netAppFolderDataResponse={netAppData}
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
    if (egressWorkspaceId !== undefined) {
      egressRefetch();
    }
  }, [egressWorkspaceId, egressRefetch]);

  useEffect(() => {
    if (netAppFolderPath) {
      netAppRefetch();
    }
  }, [netAppFolderPath, netAppRefetch]);

  return (
    <div>
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
        data-testId="transfer-main-container"
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
      <Modal
        isVisible={showTransferConfirmationModal}
        className={styles.transferConfirmationModal}
        handleClose={handleCloseTransferConfirmationModal}
        type="alert"
        ariaLabel="Document saving alert modal"
        ariaDescription="Saving updated document to CMS"
      >
        <div>
          <div className={styles.modalHeader}>
            Copy files to: 5.Correspondence
          </div>
          <div className={styles.modalContent}>
            <Checkboxes
              className="govuk-checkboxes--small"
              items={[
                {
                  children: `I confirm I want to copy ${selectedSourceFoldersOrFiles.length} folders to ${selectedTransferAction?.destinationFolder.name}`,
                },
              ]}
              name="confirmation checkbox"
            />
            <div className={styles.modalButtonWrapper}>
              <Button onClick={() => {}}>Continue</Button>
              <LinkButton onClick={handleCloseTransferConfirmationModal}>
                Cancel{" "}
              </LinkButton>
            </div>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default TransferMaterialsPage;
