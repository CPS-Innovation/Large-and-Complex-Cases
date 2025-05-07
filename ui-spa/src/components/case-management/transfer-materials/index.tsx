import { useEffect, useState, useMemo } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { LinkButton } from "../../govuk";
import FolderNavigationTable from "../../common/FolderNavigationTable";
import {
  sortByStringProperty,
  sortByDateProperty,
} from "../../../common/utils/sortUtils";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import { formatDate } from "../../../common/utils/formatDate";
import { getEgressFolders } from "../../../apis/gateway-api";
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

  const currentFolder = useMemo(() => {
    return egressPathFolders[egressPathFolders.length - 1];
  }, [egressPathFolders]);
  const egressFolderApiResults = useApi(
    getEgressFolders,
    [egressWorkspaceId, currentFolder.folderId],
    false,
  );

  const { refetch, status, data: egressData } = egressFolderApiResults;

  const egressFolderData = useMemo(() => {
    if (!egressData) return [];
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(egressData, "name", sortValues.type);

    if (sortValues?.name === "date-updated")
      return sortByDateProperty(egressData, "dateUpdated", sortValues.type);

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
    console.log("path>>", path);
    console.log("newData>>", newData);
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
    // handleGetFolderContent(path);
  };

  const getTableHeadData = () => {
    return [
      {
        children: "Folder name",
        sortable: true,
        sortName: "folder-name",
      },

      {
        children: "Date",
        sortable: true,
        sortName: "date-updated",
      },
    ];
  };

  const getTableRowData = () => {
    return egressFolderData.map((data) => {
      return {
        cells: [
          {
            children: (
              <div>
                <FolderIcon />
                <LinkButton
                  type="button"
                  onClick={() => {
                    handleFolderClick(data.id);
                  }}
                >
                  {data.name}
                </LinkButton>
              </div>
            ),
          },

          {
            children: <span>{formatDate(data.dateUpdated)}</span>,
          },
        ],
      };
    });
  };

  useEffect(() => {
    if (egressWorkspaceId !== undefined) {
      refetch();
    }
  }, [egressWorkspaceId, refetch]);

  return (
    <div>
      <h2>transfer materials</h2>
      <div className={styles.mainContainer}>
        <div className={styles.egressContainer}>
          <div className={styles.titleWrapper}>
            <h3>Egress Inbound documents</h3>
          </div>
          <FolderNavigationTable
            folders={egressPathFolders}
            loaderText="Loading folders from Egress"
            folderResultsStatus={status}
            folderResultsLength={egressFolderData.length}
            handleFolderPathClick={handleFolderPathClick}
            getTableRowData={getTableRowData}
            getTableHeadData={getTableHeadData}
            handleTableSort={handleTableSort}
          />
        </div>
        <div className={styles.netappContainer}>
          <div className={styles.titleWrapper}>
            <h3>Shared drive</h3>
          </div>
          netapp data
        </div>
      </div>
    </div>
  );
};

export default TransferMaterialsPage;
