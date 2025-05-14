import { useMemo, useState } from "react";
import { LinkButton } from "../../govuk";
import { NetAppFolderData } from "../../../common/types/NetAppFolderData";
import { sortByStringProperty } from "../../../common/utils/sortUtils";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import FolderNavigationTable from "../../common/FolderNavigationTable";
import FolderIcon from "../../../components/svgs/folder.svg?react";
// import styles from "./netAppFolderResultsPage.module.scss";

type NetAppFolderContainerProps = {
  rootFolderPath: string;
  netAppFolderData?: NetAppFolderData;
  netAppFolderDataStatus: "loading" | "succeeded" | "failed" | "initial";
  handleGetFolderContent: (folderId: string) => void;
};

const NetAppFolderContainer: React.FC<NetAppFolderContainerProps> = ({
  rootFolderPath,
  netAppFolderData,
  netAppFolderDataStatus,
  handleGetFolderContent,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

  const netAppFolderDataSorted = useMemo(() => {
    if (!netAppFolderData?.folders) return [];
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(
        netAppFolderData.folders,
        "folderPath",
        sortValues.type,
      );

    return netAppFolderData.folders;
  }, [netAppFolderData, sortValues]);

  const folders = useMemo(() => {
    const parts = rootFolderPath.split("/").filter(Boolean);

    const result = parts.map((folderName, index) => ({
      folderName,
      folderPath: `${parts.slice(0, index + 1).join("/")}/`,
    }));
    const withHome = [{ folderName: "Home", folderPath: "" }, ...result];
    return withHome;
  }, [rootFolderPath]);

  const getTableRowData = () => {
    return netAppFolderDataSorted.map((data) => {
      return {
        cells: [
          {
            children: (
              <div>
                <FolderIcon />
                <LinkButton
                  type="button"
                  onClick={() => {
                    handleGetFolderContent(data.folderPath);
                  }}
                >
                  {getFolderNameFromPath(data.folderPath)}
                </LinkButton>
              </div>
            ),
          },
        ],
      };
    });
  };

  const getTableHeadData = () => {
    return [
      {
        children: <>Folder name</>,
        sortable: true,
        sortName: "folder-name",
      },

      {
        children: <></>,
        sortable: false,
      },
    ];
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
    <div className={`govuk-width-container `}>
      <div className={"govuk-grid-column-two-thirds"}>
        <FolderNavigationTable
          tableName={"netapp"}
          folders={folders}
          loaderText="Loading folders from Network Shared Drive"
          folderResultsStatus={netAppFolderDataStatus}
          folderResultsLength={netAppFolderDataSorted.length}
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
