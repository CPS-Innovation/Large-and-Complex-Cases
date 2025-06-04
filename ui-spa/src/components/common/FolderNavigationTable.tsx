import { SortableTable } from "../govuk";
import { Spinner } from "../common/Spinner";
import FolderPath, { Folder } from "../common/FolderPath";
import styles from "./FolderNavigationTable.module.scss";

type FolderNavigationTableProps = {
  tableName: string;
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
    children: React.ReactElement;
    sortable: boolean;
    sortName?: string;
  }[];
  handleTableSort: (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => void;
  showInsetElement?: boolean;
  getInsetElement?: () => React.ReactElement;
};

const FolderNavigationTable: React.FC<FolderNavigationTableProps> = ({
  tableName,
  folders,
  loaderText,
  folderResultsStatus,
  folderResultsLength,
  handleFolderPathClick,
  getTableRowData,
  getTableHeadData,
  handleTableSort,
  showInsetElement,
  getInsetElement,
}) => {
  return (
    <div className={styles.results} data-testId={`${tableName}-table-wrapper`}>
      <div>
        {
          <FolderPath
            folders={folders}
            disabled={folderResultsStatus === "loading"}
            handleFolderPathClick={handleFolderPathClick}
          />
        }
        {showInsetElement && getInsetElement && getInsetElement()}
        {folderResultsStatus === "succeeded" && (
          <>
            <SortableTable
              head={getTableHeadData()}
              rows={getTableRowData()}
              handleTableSort={handleTableSort}
            />
            {!folderResultsLength && (
              <p>There are no documents currently in this folder</p>
            )}
          </>
        )}
        {folderResultsStatus === "loading" && (
          <div className={styles.spinnerWrapper}>
            <Spinner
              data-testid={`${tableName}-folder-table-loader`}
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
