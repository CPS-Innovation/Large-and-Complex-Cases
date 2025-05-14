import { useEffect, useState, useMemo } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { LinkButton, InsetText } from "../../govuk";
import Checkbox from "../../common/Checkbox";
import FolderNavigationTable from "../../common/FolderNavigationTable";
import {
  sortByStringProperty,
  sortByDateProperty,
} from "../../../common/utils/sortUtils";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import NetAppFolderContainer from "./NetAppFolderContainer";
import { formatDate } from "../../../common/utils/formatDate";
import { formatFileSize } from "../../../common/utils/formatFileSize";
import { getEgressFolders, getNetAppFolders } from "../../../apis/gateway-api";
import styles from "./index.module.scss";

type TransferMaterialsPageProps = {
  egressWorkspaceId: string | undefined;
  netappFolderPath: string | undefined;
};

const TransferMaterialsPage: React.FC<TransferMaterialsPageProps> = ({
  egressWorkspaceId,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();
  const [egressPathFolders, setEgressPathFolders] = useState<
    {
      folderName: string;
      folderPath: string;
      folderId?: string;
    }[]
  >([{ folderName: "Home", folderPath: "", folderId: "" }]);

  const [switchSource, setSwitchSource] = useState(false);
  const [netAppFolderPath, setNetAppFolderPath] = useState("");

  const [selectedEgressFolders, setSelectedEgressFolders] = useState<string[]>(
    [],
  );

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

  const egressFolderData = useMemo(() => {
    if (!egressData) return [];
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(egressData, "name", sortValues.type);

    if (sortValues?.name === "date-updated")
      return sortByDateProperty(egressData, "dateUpdated", sortValues.type);

    if (sortValues?.name === "file-size")
      return sortByStringProperty(egressData, "filesize", sortValues.type);

    return egressData;
  }, [egressData, sortValues]);

  const handleTableSort = (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => {
    setSortValues({ name: sortName, type: sortType });
  };

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
    const folderData = egressFolderData.find((item) => item.id === id);
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
        updatedFolders = [
          "all-folders",
          ...egressFolderData.map((data) => data.id),
        ];
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

  const getTableHeadData = () => {
    return [
      {
        children: (
          <>
            <Checkbox
              id={"all-folders"}
              checked={isEgressFolderChecked("all-folders")}
              onChange={handleCheckboxChange}
              ariaLabel="Select all folders"
            />
          </>
        ),
        sortable: false,
      },
      {
        children: <>Folder/file name</>,
        sortable: true,
        sortName: "folder-name",
      },

      {
        children: <>Last modified date</>,
        sortable: true,
        sortName: "date-updated",
      },
      {
        children: <>Size</>,
        sortable: true,
        sortName: "file-size",
      },
    ];
  };

  const getTableRowData = () => {
    return egressFolderData.map((data) => {
      return {
        cells: [
          {
            children: (
              <>
                <Checkbox
                  id={data.id}
                  checked={isEgressFolderChecked(data.id)}
                  onChange={handleCheckboxChange}
                  ariaLabel="select folder"
                />
              </>
            ),
          },
          {
            children: (
              <div className={styles.iconButtonWrapper}>
                {data.isFolder ? (
                  <>
                    <FolderIcon />
                    <LinkButton
                      type="button"
                      onClick={() => {
                        handleFolderClick(data.id);
                      }}
                    >
                      {data.name}
                    </LinkButton>
                  </>
                ) : (
                  <>
                    <FileIcon />
                    <span className={styles.fileName}>{data.name}</span>
                  </>
                )}
              </div>
            ),
          },

          {
            children: <span>{formatDate(data.dateUpdated)}</span>,
          },
          {
            children: (
              <span>{data.filesize ? formatFileSize(data.filesize) : ""}</span>
            ),
          },
        ],
      };
    });
  };

  useEffect(() => {
    if (egressWorkspaceId !== undefined) {
      egressRefetch();
    }
  }, [egressWorkspaceId, egressRefetch]);

  useEffect(() => {
    if (netAppFolderPath !== undefined) {
      netAppRefetch();
    }
  }, [netAppFolderPath, netAppRefetch]);

  const handleSwitchSource = () => {
    setSwitchSource(!switchSource);
  };

  const renderEgressContainer = () => {
    return (
      <div className={styles.egressContainer} data-testId="egress-container">
        <div className={styles.titleWrapper}>
          <h3>Egress Inbound documents</h3>
        </div>
        <div className={styles.tableContainer}>
          <FolderNavigationTable
            tableName={"egress"}
            folders={egressPathFolders}
            loaderText="Loading folders from Egress"
            folderResultsStatus={egressStatus}
            folderResultsLength={egressFolderData.length}
            handleFolderPathClick={handleFolderPathClick}
            getTableRowData={getTableRowData}
            getTableHeadData={getTableHeadData}
            handleTableSort={handleTableSort}
          />
        </div>
      </div>
    );
  };

  const renderNetappContainer = () => {
    return (
      <div className={styles.netappContainer} data-testId="netapp-container">
        <div className={styles.titleWrapper}>
          <h3>Shared drive</h3>
        </div>
        <div className={styles.tableContainer}>
          <NetAppFolderContainer
            rootFolderPath={netAppFolderPath}
            netAppFolderDataStatus={netAppStatus}
            netAppFolderData={netAppData}
            handleGetFolderContent={handleGetFolderContent}
          />
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
        {switchSource ? (
          <>
            {renderNetappContainer()}
            {renderEgressContainer()}
          </>
        ) : (
          <>
            {renderEgressContainer()}
            {renderNetappContainer()}
          </>
        )}
      </div>
    </div>
  );
};

export default TransferMaterialsPage;
