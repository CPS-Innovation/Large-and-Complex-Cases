import { SortableTable } from "../govuk";
import { Spinner } from "../common/Spinner";
import { useMemo } from "react";
import FolderPath, { Folder } from "../common/FolderPath";
import styles from "./FolderNavigationTable.module.scss";

type FolderNavigationTableProps = {
  caption: string;
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
  caption,
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
  const showInset = useMemo(() => {
    return (
      folderResultsStatus === "succeeded" && showInsetElement && getInsetElement
    );
  }, [folderResultsStatus, showInsetElement, getInsetElement]);

  const statusText = useMemo(() => {
    const tableType = tableName === "egress" ? "egress" : "shared drive";
    const folderText =
      folders.length > 1
        ? `folder ${folders[folders.length - 1].folderName}`
        : "";
    if (folderResultsStatus === "loading") {
      return `loading files and folders from ${tableType}  ${folderText}`;
    }
    if (folderResultsStatus === "succeeded") {
      return folderResultsLength
        ? "files and folders loaded successfully"
        : "There are no documents currently in this folder";
    }
    return "";
  }, [folderResultsStatus, folders, tableName, folderResultsLength]);
  return (
    <div className={styles.results} data-testid={`${tableName}-table-wrapper`}>
      <div aria-live="polite" className="govuk-visually-hidden">
        {statusText}
      </div>
      <div>
        {
          <FolderPath
            folders={folders}
            disabled={folderResultsStatus === "loading"}
            handleFolderPathClick={handleFolderPathClick}
          />
        }
        {showInset && getInsetElement!()}
        {folderResultsStatus === "succeeded" && (
          <>
            <SortableTable
              captionClassName="govuk-visually-hidden"
              caption={caption}
              head={getTableHeadData()}
              rows={getTableRowData()}
              handleTableSort={handleTableSort}
            />
            {!folderResultsLength && (
              <p data-testid="no-documents-text">
                There are no documents currently in this folder
              </p>
            )}
          </>
        )}
        {folderResultsStatus === "loading" && (
          <div className={styles.spinnerWrapper}>
            <Spinner
              data-testid={`${tableName}-folder-table-loader`}
              diameterPx={50}
            />
            <div className={styles.spinnerText} aria-live="polite">
              {loaderText}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default FolderNavigationTable;
