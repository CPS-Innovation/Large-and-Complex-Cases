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

import styles from "./EgressFolderContainer.module.scss";

type TransferSourceNavigationTableContainerProps = {
  folderData:
    | { type: "egress"; data: EgressFolderData }
    | { type: "netapp"; data: NetAppFolderData };
  isLoading: boolean;
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
  handleFolderClick,
  handleTableSort,
  handleCheckboxChange,
  isSourceFolderChecked,
}) => {
  //   const [sortValues, setSortValues] = useState<{
  //     name: string;
  //     type: "ascending" | "descending";
  //   }>();

  //   const egressDataSorted = useMemo(() => {
  //     if (sortValues?.name === "folder-name")
  //       return sortByStringProperty(egressFolderData, "name", sortValues.type);

  //     if (sortValues?.name === "date-updated")
  //       return sortByDateProperty(
  //         egressFolderData,
  //         "dateUpdated",
  //         sortValues.type,
  //       );

  //     if (sortValues?.name === "file-size")
  //       return sortByNumberProperty(
  //         egressFolderData,
  //         "filesize",
  //         sortValues.type,
  //       );

  //     return egressFolderData;
  //   }, [egressFolderData, sortValues]);

  //   const handleTableSort = (
  //     sortName: string,
  //     sortType: "ascending" | "descending",
  //   ) => {
  //     setSortValues({ name: sortName, type: sortType });
  //   };

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
            children: <span>{formatDate(data?.dateUpdated)}</span>,
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

  const hideFirstColumn = useMemo(() => {
    const { data, type } = folderData;
    if (!data.length) return true;
    //below condition is to check for the root egress folder
    return type === "egress" ? data[0]?.path === `${data[0]?.name}/` : false;
  }, [folderData]);

  return (
    <div className={hideFirstColumn ? styles.hideFirstColumn : ""}>
      <NavigationTable
        caption="egress files and folders table, column headers with buttons are sortable"
        tableName="egress"
        loaderText="Loading folders from Egress"
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
