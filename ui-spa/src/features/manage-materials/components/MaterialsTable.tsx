import { SortableTable } from "../../../components/govuk";
import Checkbox from "../../../components/common/Checkbox";
import { Spinner } from "../../../components/common/Spinner";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import { type NetAppFolderData, type NetAppFileFolder } from "../../../schemas";
import styles from "./MaterialsTable.module.scss";

export const SELECT_ALL_CHECKBOX_ID = "select-all-materials";

type MaterialsTableProps = {
  items: NetAppFolderData;
  isLoading: boolean;
  selectedPaths: string[];
  handleCheckboxChange: (id: string, checked: boolean) => void;
  handleFolderClick: (item: NetAppFileFolder) => void;
  handleTableSort: (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => void;
};

const MaterialsTable: React.FC<MaterialsTableProps> = ({
  items,
  isLoading,
  selectedPaths,
  handleCheckboxChange,
  handleFolderClick,
  handleTableSort,
}) => {
  const areAllItemsSelected =
    items.length > 0 && items.every(({ path }) => selectedPaths.includes(path));

  const getTableHeadData = () => [
    {
      children: items.length ? (
        <div className="govuk-checkboxes govuk-checkboxes--small">
          <Checkbox
            id={SELECT_ALL_CHECKBOX_ID}
            checked={areAllItemsSelected}
            onChange={handleCheckboxChange}
            ariaLabel="Select all files and folders"
            data-testid="select-all-materials-checkbox"
          />
        </div>
      ) : (
        <></>
      ),
      sortable: false,
    },
    {
      children: <>File or folder</>,
      sortable: true,
      sortName: "name",
    },
  ];

  const getTableRowData = () =>
    items.map((item) => ({
      reactListKey: item.path,
      cells: [
        {
          children: (
            <div className="govuk-checkboxes govuk-checkboxes--small">
              <Checkbox
                id={item.path}
                checked={selectedPaths.includes(item.path)}
                onChange={handleCheckboxChange}
                ariaLabel={
                  item.isFolder
                    ? `Select folder ${item.name}`
                    : `Select file ${item.name}`
                }
                data-testid={`material-checkbox-${item.name}`}
              />
            </div>
          ),
        },
        {
          children: item.isFolder ? (
            <button
              type="button"
              className={`govuk-link govuk-link--no-visited-state ${styles.folderOpen}`}
              onClick={() => handleFolderClick(item)}
            >
              <FolderIcon
                className={styles.folderIcon}
                data-testid="folder-icon"
                aria-hidden="true"
              />
              <span className={styles.folderName}>{item.name}</span>
            </button>
          ) : (
            <span className={styles.fileName}>{item.name}</span>
          ),
        },
      ],
    }));

  return (
    <div className={styles.materialsTableWrapper}>
      <div aria-live="polite" className="govuk-visually-hidden">
        {isLoading
          ? "Loading folders from Shared Drive"
          : "files and folders loaded successfully"}
      </div>
      {isLoading ? (
        <div className={styles.spinnerWrapper} data-testid="materials-loader">
          <Spinner diameterPx={50} />
          <div className={styles.spinnerText}>
            Loading folders from Shared Drive
          </div>
        </div>
      ) : (
        <>
          <SortableTable
            className={styles.materialsTable}
            captionClassName="govuk-visually-hidden"
            caption="shared drive materials table, column headers with buttons are sortable"
            head={getTableHeadData()}
            rows={getTableRowData()}
            handleTableSort={handleTableSort}
            data-testid="materials-table"
          />
          {!items.length && (
            <p className="govuk-body" data-testid="no-materials-text">
              There are no documents currently in this folder
            </p>
          )}
        </>
      )}
    </div>
  );
};

export default MaterialsTable;
