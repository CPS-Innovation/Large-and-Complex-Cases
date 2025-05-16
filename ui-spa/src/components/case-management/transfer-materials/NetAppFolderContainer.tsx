import { useMemo, useState } from "react";
import { LinkButton } from "../../govuk";
import Checkbox from "../../common/Checkbox";
import { NetAppFolderDataResponse } from "../../../common/types/NetAppFolderData";
import { sortByStringProperty } from "../../../common/utils/sortUtils";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import FolderNavigationTable from "../../common/FolderNavigationTable";
import { formatFileSize } from "../../../common/utils/formatFileSize";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { mapToNetAppFolderData } from "../../../common/utils/mapToNetAppFolderData";
import styles from "./netAppFolderContainer.module.scss";

type NetAppFolderContainerProps = {
  transferSource: "egress" | "netapp";
  connectedFolderPath: string;
  currentFolderPath: string;
  netAppFolderDataResponse?: NetAppFolderDataResponse;
  netAppFolderDataStatus: "loading" | "succeeded" | "failed" | "initial";
  handleGetFolderContent: (folderId: string) => void;
  handleCheckboxChange: (id: string, checked: boolean) => void;
  isEgressFolderChecked: (id: string) => boolean;
};

const NetAppFolderContainer: React.FC<NetAppFolderContainerProps> = ({
  transferSource,
  connectedFolderPath,
  currentFolderPath,
  netAppFolderDataResponse,
  netAppFolderDataStatus,
  handleGetFolderContent,
  handleCheckboxChange,
  isEgressFolderChecked,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

  const netAppFolderData = useMemo(
    () =>
      netAppFolderDataResponse
        ? mapToNetAppFolderData(netAppFolderDataResponse)
        : [],
    [netAppFolderDataResponse],
  );

  const netAppDataSorted = useMemo(() => {
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(netAppFolderData, "path", sortValues.type);
    if (sortValues?.name === "file-size")
      return sortByStringProperty(
        netAppFolderData,
        "filesize",
        sortValues.type,
      );

    return netAppFolderData;
  }, [netAppFolderData, sortValues]);

  const folders = useMemo(() => {
    const replacedString = currentFolderPath.replace(connectedFolderPath, "");
    const parts = replacedString.split("/").filter(Boolean);

    const result = parts.map((folderName, index) => ({
      folderName,
      folderPath: `${connectedFolderPath}${parts.slice(0, index + 1).join("/")}/`,
    }));
    const withHome = [
      { folderName: "Home", folderPath: connectedFolderPath },
      ...result,
    ];
    return withHome;
  }, [currentFolderPath, connectedFolderPath]);

  const getTableRowData = () => {
    const rowData = netAppDataSorted.map((data) => {
      return {
        cells: [
          {
            children: (
              <>
                <Checkbox
                  id={data.path}
                  checked={isEgressFolderChecked(data.path)}
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
                        handleGetFolderContent(data.path);
                      }}
                    >
                      {getFolderNameFromPath(data.path)}
                    </LinkButton>
                  </>
                ) : (
                  <>
                    <FileIcon />
                    <span className={styles.fileName}>
                      {getFolderNameFromPath(data.path)}
                    </span>
                  </>
                )}
              </div>
            ),
          },
          {
            children: (
              <span>{data.filesize ? formatFileSize(data.filesize) : ""}</span>
            ),
          },
        ],
      };
    });
    if (transferSource !== "netapp") {
      const filteredRowData = rowData.map((data) => {
        return {
          cells: data.cells.slice(1),
        };
      });
      return filteredRowData;
    }
    return rowData;
  };

  const getTableHeadData = () => {
    const tableHeadData = [
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
        children: <>Folder name</>,
        sortable: true,
        sortName: "folder-name",
      },

      {
        children: <>Size</>,
        sortable: true,
        sortName: "file-size",
      },
    ];
    if (transferSource !== "netapp") return tableHeadData.slice(1);
    return tableHeadData;
  };

  const handleTableSort = (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => {
    setSortValues({ name: sortName, type: sortType });
  };

  const handleFolderPathClick = (path: string) => {
    handleGetFolderContent(path);
  };

  return (
    <div>
      <div>
        <FolderNavigationTable
          tableName={"netapp"}
          folders={folders}
          loaderText="Loading folders from Network Shared Drive"
          folderResultsStatus={netAppFolderDataStatus}
          folderResultsLength={netAppDataSorted.length}
          handleFolderPathClick={handleFolderPathClick}
          getTableRowData={getTableRowData}
          getTableHeadData={getTableHeadData}
          handleTableSort={handleTableSort}
        />
      </div>
    </div>
  );
};

export default NetAppFolderContainer;
