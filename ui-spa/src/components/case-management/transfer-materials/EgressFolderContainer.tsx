import { useMemo, useState } from "react";
import { LinkButton } from "../../govuk";
import Checkbox from "../../common/Checkbox";
import FolderNavigationTable from "../../common/FolderNavigationTable";
import {
  sortByStringProperty,
  sortByDateProperty,
} from "../../../common/utils/sortUtils";
import { formatFileSize } from "../../../common/utils/formatFileSize";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { formatDate } from "../../../common/utils/formatDate";
import { EgressFolderData } from "../../../common/types/EgressFolderData";
import styles from "./egressFolderContainer.module.scss";

type EgressFolderContainerProps = {
  egressData?: EgressFolderData;
  egressDataStatus: "loading" | "succeeded" | "failed" | "initial";
  egressPathFolders: {
    folderName: string;
    folderPath: string;
    folderId?: string;
  }[];
  handleFolderPathClick: (path: string) => void;
  handleFolderClick: (id: string) => void;
  handleCheckboxChange: (id: string, checked: boolean) => void;
  isEgressFolderChecked: (id: string) => boolean;
};

const EgressFolderContainer: React.FC<EgressFolderContainerProps> = ({
  egressData,
  egressDataStatus,
  egressPathFolders,
  handleFolderPathClick,
  handleFolderClick,
  handleCheckboxChange,
  isEgressFolderChecked,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

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
  return (
    <div>
      <FolderNavigationTable
        tableName="egress"
        folders={egressPathFolders}
        loaderText="Loading folders from Egress"
        folderResultsStatus={egressDataStatus}
        folderResultsLength={egressFolderData.length}
        handleFolderPathClick={handleFolderPathClick}
        getTableRowData={getTableRowData}
        getTableHeadData={getTableHeadData}
        handleTableSort={handleTableSort}
      />
    </div>
  );
};

export default EgressFolderContainer;
