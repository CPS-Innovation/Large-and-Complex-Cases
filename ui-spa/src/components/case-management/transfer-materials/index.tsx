import { useEffect, useState, useMemo } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { LinkButton, InsetText } from "../../govuk";
import NetAppFolderContainer from "./NetAppFolderContainer";
import { getEgressFolders, getNetAppFolders } from "../../../apis/gateway-api";
import EgressFolderContainer from "./EgressFolderContainer";
import styles from "./index.module.scss";

type TransferMaterialsPageProps = {
  egressWorkspaceId: string | undefined;
  netAppPath: string | undefined;
};

const TransferMaterialsPage: React.FC<TransferMaterialsPageProps> = ({
  egressWorkspaceId,
  netAppPath,
}) => {
  const [egressPathFolders, setEgressPathFolders] = useState<
    {
      folderName: string;
      folderPath: string;
      folderId?: string;
    }[]
  >([{ folderName: "Home", folderPath: "", folderId: "" }]);

  const [transferSource, setTransferSource] = useState<"egress" | "netapp">(
    "egress",
  );
  const [netAppFolderPath, setNetAppFolderPath] = useState("");

  const [selectedEgressFolders, setSelectedEgressFolders] = useState<string[]>(
    [],
  );
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
  } = useApi(
    getEgressFolders,
    [egressWorkspaceId, currentEgressFolder.folderId],
    false,
  );

  const {
    refetch: netAppRefetch,
    status: netAppStatus,
    data: netAppData,
  } = useApi(getNetAppFolders, [netAppFolderPath], false);

  const handleFolderPathClick = (path: string) => {
    const index = egressPathFolders.findIndex(
      (item) => item.folderPath === path,
    );
    const newData =
      index !== -1
        ? egressPathFolders.slice(0, index + 1)
        : [...egressPathFolders];
    setEgressPathFolders(newData);
  };

  const handleFolderClick = (id: string) => {
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
  };

  const handleCheckboxChange = (id: string, checked: boolean) => {
    let updatedFolders: string[] = [];

    if (id === "all-folders") {
      if (checked) {
        updatedFolders = ["all-folders", ...egressData!.map((data) => data.id)];
      } else {
        updatedFolders = [];
      }
    } else if (!checked) {
      updatedFolders = selectedEgressFolders.filter((item) => item !== id);
    } else {
      updatedFolders = [...selectedEgressFolders, id];
    }

    setSelectedEgressFolders(updatedFolders);
  };

  const isEgressFolderChecked = (id: string) => {
    return selectedEgressFolders.includes(id);
  };

  const handleGetFolderContent = (path: string) => {
    setNetAppFolderPath(path);
  };

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

  const handleSwitchSource = () => {
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
              handleFolderPathClick={handleFolderPathClick}
              handleFolderClick={handleFolderClick}
              handleCheckboxChange={handleCheckboxChange}
              isEgressFolderChecked={isEgressFolderChecked}
            />
          }
        </div>
      </div>
    );
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
              handleGetFolderContent={handleGetFolderContent}
              handleCheckboxChange={handleCheckboxChange}
              isEgressFolderChecked={isEgressFolderChecked}
            />
          )}
        </div>
      </div>
    );
  };

  return (
    <div>
      <h2>Transfer folders and files between egress and the shared drive</h2>
      <InsetText>
        Select the folders and files you want to transfer, then choose a
        destination. You can switch the source and destination if needed.{" "}
        <LinkButton onClick={handleSwitchSource}> Switch source</LinkButton>
      </InsetText>
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
    </div>
  );
};

export default TransferMaterialsPage;
