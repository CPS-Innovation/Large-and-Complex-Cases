import { SortableTable } from "../../govuk";
import { Spinner } from "../../common/Spinner";
import { useMemo } from "react";
import styles from "./NavigationTable.module.scss";

type NavigationTableProps = {
  caption: string;
  tableName: string;
  isLoading: boolean;
  folderResultsLength: number;
  loaderText: string;
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
};

const NavigationTable: React.FC<NavigationTableProps> = ({
  caption,
  tableName,
  loaderText,
  isLoading,
  folderResultsLength,
  getTableRowData,
  getTableHeadData,
  handleTableSort,
}) => {
  const statusText = useMemo(() => {
    if (isLoading) {
      return loaderText;
    }
    if (!isLoading) {
      return folderResultsLength
        ? "files and folders loaded successfully"
        : "There are no documents currently in this folder";
    }
    return "";
  }, [isLoading, loaderText, folderResultsLength]);
  return (
    <div
      className={styles.navigationTable}
      data-testid={`${tableName}-table-wrapper`}
    >
      <div aria-live="polite" className="govuk-visually-hidden">
        {statusText}
      </div>
      <div>
        {isLoading ? (
          <div className={styles.spinnerWrapper}>
            <Spinner
              data-testid={`${tableName}-folder-table-loader`}
              diameterPx={50}
            />
            <div className={styles.spinnerText} aria-live="polite">
              {loaderText}
            </div>
          </div>
        ) : (
          <div>
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
          </div>
        )}
      </div>
    </div>
  );
};

export default NavigationTable;
