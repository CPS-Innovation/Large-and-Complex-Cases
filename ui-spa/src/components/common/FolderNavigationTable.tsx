import { SortableTable } from "../govuk";
import { Spinner } from "../common/Spinner";
import FolderPath, { Folder } from "../common/FolderPath";
import styles from "./FolderNavigationTable.module.scss";

type FolderNavigationTableProps = {
  folders: Folder[];
  folderResultsStatus: "loading" | "succeeded" | "failed" | "initial";
  folderResultsLength: number;
  loaderText: string;
  handleFolderPathClick: (folderPath: string) => void;
  getTableRowData: () => {
    cells: {
      children: React.ReactElement;
    }[];
  }[];
  getTableHeadData: () => {
    children: string;
    sortable: boolean;
    sortName?: string;
  }[];
  handleTableSort: (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => void;
};

const FolderNavigationTable: React.FC<FolderNavigationTableProps> = ({
  folders,
  loaderText,
  folderResultsStatus,
  folderResultsLength,
  handleFolderPathClick,
  getTableRowData,
  getTableHeadData,
  handleTableSort,
}) => {
  return (
    <div className={styles.results}>
      <div>
        {
          <FolderPath
            folders={folders}
            disabled={folderResultsStatus === "loading"}
            handleFolderPathClick={handleFolderPathClick}
          />
        }
        {folderResultsStatus === "succeeded" && (
          <>
            <SortableTable
              head={getTableHeadData()}
              rows={getTableRowData()}
              handleTableSort={handleTableSort}
            />
            {!folderResultsLength && (
              <p>There are no documents currenlty in this folder</p>
            )}
          </>
        )}
        {folderResultsStatus === "loading" && (
          <div className={styles.spinnerWrapper}>
            <Spinner
              data-testid="netapp-folder-loader"
              diameterPx={50}
              ariaLabel={loaderText}
            />
            <div className={styles.spinnerText}>{loaderText}</div>
          </div>
        )}
      </div>
    </div>
  );
};

export default FolderNavigationTable;
