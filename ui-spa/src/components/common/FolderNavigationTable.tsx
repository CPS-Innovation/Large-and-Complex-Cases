import { SortableTable } from "../govuk";
import { Spinner } from "../common/Spinner";
import FolderPath from "../common/FolderPath";
import styles from "./FolderNavigationTable.module.scss";

type FolderNavigationTableProps = {
  rootFolderPath: string;
  folderResultsStatus: "loading" | "succeeded" | "failed" | "initial";
  folderResultsLength: number;
  loaderText: string;
  handleFolderClick: (folderPath: string) => void;
  getTableRowData: () => {
    cells: {
      children: React.ReactElement;
    }[];
  }[];
  handleTableSort: (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => void;
};

const FolderNavigationTable: React.FC<FolderNavigationTableProps> = ({
  rootFolderPath,
  loaderText,
  folderResultsStatus,
  folderResultsLength,
  handleFolderClick,
  getTableRowData,
  handleTableSort,
}) => {
  return (
    <div className={styles.results}>
      <div>
        {
          <FolderPath
            path={rootFolderPath}
            disabled={folderResultsStatus === "loading"}
            folderClickHandler={handleFolderClick}
          />
        }
        {folderResultsStatus === "succeeded" && (
          <>
            <SortableTable
              head={[
                {
                  children: "Folder name",
                  sortable: true,
                  sortName: "folder-name",
                },

                {
                  children: "",
                  sortable: false,
                },
              ]}
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
