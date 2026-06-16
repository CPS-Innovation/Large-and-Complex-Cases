import { useMemo } from "react";
import { LinkButton } from "../../govuk";
import Checkbox from "../../common/Checkbox";
import NavigationTable from "./NavigationTable";
import { formatFileSize } from "../../../common/utils/formatFileSize";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { formatDate } from "../../../common/utils/formatDate";
import {
  type EgressFolderData,
  type NetAppFolderData,
  type EgressFolder,
  type NetAppFileFolder,
} from "../../../schemas";

import styles from "./TransferSourceNavigationTableContainer.module.scss";

type TransferSourceNavigationTableContainerProps = {
  folderData:
    | { type: "egress"; data: EgressFolderData }
    | { type: "netapp"; data: NetAppFolderData };
  isLoading: boolean;
  hideCheckboxesColumn: boolean;
  handleFolderClick: (data: EgressFolder | NetAppFileFolder) => void;
  handleTableSort: (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => void;
  handleCheckboxChange: (id: string, checked: boolean) => void;
  isSourceFolderChecked: (checkboxId: string) => boolean;
};

const TransferSourceNavigationTableContainer: React.FC<
  TransferSourceNavigationTableContainerProps
> = ({
  folderData,
  isLoading,
  hideCheckboxesColumn,
  handleFolderClick,
  handleTableSort,
  handleCheckboxChange,
  isSourceFolderChecked,
}) => {
  const getTableHeadData = () => {
    const tableHeadData = [
      {
        children: (
          <Checkbox
            id={"all-folders"}
            checked={isSourceFolderChecked("all-folders")}
            onChange={handleCheckboxChange}
            ariaLabel="Select folders and files"
          />
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
    return tableHeadData;
  };

  const getTableRowData = () => {
    const rowData = folderData.data.map((data) => {
      return {
        cells: [
          {
            children: (
              <Checkbox
                id={data.path}
                checked={isSourceFolderChecked(data.path)}
                onChange={handleCheckboxChange}
                ariaLabel={
                  data.isFolder
                    ? `select folder ${data.name}`
                    : `select file ${data.name}`
                }
              />
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
                        handleFolderClick(data);
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
            children: (
              <span>
                {"dateUpdated" in data
                  ? formatDate(data.dateUpdated)
                  : formatDate(data.lastModified)}
              </span>
            ),
          },
          {
            children: (
              <span>
                {data.filesize ? formatFileSize(data.filesize) : "--"}
              </span>
            ),
          },
        ],
      };
    });
    return rowData;
  };

  return (
    <div
      className={
        hideCheckboxesColumn
          ? `${styles.sourceContainer} ${styles.hideFirstColumn}`
          : styles.sourceContainer
      }
    >
      <NavigationTable
        caption={
          folderData.type === "egress"
            ? "egress files and folders table, column headers with buttons are sortable"
            : "shared drive files and folders table, column headers with buttons are sortable"
        }
        tableName={folderData.type === "egress" ? "egress" : "shared drive"}
        loaderText={
          folderData.type === "egress"
            ? "Loading folders from Egress"
            : "Loading folders from Shared Drive"
        }
        isLoading={isLoading}
        folderResultsLength={folderData.data.length}
        getTableRowData={getTableRowData}
        getTableHeadData={getTableHeadData}
        handleTableSort={handleTableSort}
      />
    </div>
  );
};

export default TransferSourceNavigationTableContainer;
