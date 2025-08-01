import { useMemo, useState } from "react";
import { LinkButton, InsetText } from "../../govuk";
import Checkbox from "../../common/Checkbox";
import FolderNavigationTable from "../../common/FolderNavigationTable";
import {
  sortByStringProperty,
  sortByDateProperty,
} from "../../../common/utils/sortUtils";
import { formatFileSize } from "../../../common/utils/formatFileSize";
import { getActionDataFromId } from "../../../common/utils/getActionDataFromId";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { formatDate } from "../../../common/utils/formatDate";
import { EgressFolderData } from "../../../common/types/EgressFolderData";
import { DropdownButton } from "../../common/DropdownButton";
import { TransferAction } from "../../../common/types/TransferAction";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";

import styles from "./EgressFolderContainer.module.scss";

type EgressFolderContainerProps = {
  transferSource: "egress" | "netapp";
  egressFolderData: EgressFolderData;
  egressDataStatus: "loading" | "succeeded" | "failed" | "initial";
  egressPathFolders: {
    folderName: string;
    folderPath: string;
    folderId: string;
  }[];
  selectedSourceLength: number;
  handleFolderPathClick: (path: string) => void;
  handleFolderClick: (id: string) => void;
  handleCheckboxChange: (id: string, checked: boolean) => void;
  isSourceFolderChecked: (checkboxId: string) => boolean;
  handleSelectedActionType: (transferAction: TransferAction) => void;
};

const EgressFolderContainer: React.FC<EgressFolderContainerProps> = ({
  transferSource,
  egressFolderData,
  egressDataStatus,
  egressPathFolders,
  selectedSourceLength,
  handleFolderPathClick,
  handleFolderClick,
  handleCheckboxChange,
  isSourceFolderChecked,
  handleSelectedActionType,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

  const egressDataSorted = useMemo(() => {
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(egressFolderData, "name", sortValues.type);

    if (sortValues?.name === "date-updated")
      return sortByDateProperty(
        egressFolderData,
        "dateUpdated",
        sortValues.type,
      );

    if (sortValues?.name === "file-size")
      return sortByStringProperty(
        egressFolderData,
        "filesize",
        sortValues.type,
      );

    return egressFolderData;
  }, [egressFolderData, sortValues]);

  const handleTableSort = (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => {
    setSortValues({ name: sortName, type: sortType });
  };

  const getTableSourceHeadData = () => {
    const tableHeadData = [
      {
        children: (
          <Checkbox
            id={"all-folders"}
            checked={isSourceFolderChecked("all-folders")}
            onChange={handleCheckboxChange}
            ariaLabel="Select all folders"
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

  const getTableDestinationHeadData = () => {
    const tableHeadData = [
      {
        children: <>Folder/file name</>,
        sortable: true,
        sortName: "folder-name",
      },

      {
        children: <>Size</>,
        sortable: true,
        sortName: "file-size",
      },
    ];
    if (selectedSourceLength) {
      return [
        ...tableHeadData,
        {
          children: <></>,
          sortable: false,
        },
      ];
    }
    return tableHeadData;
  };

  const getTableHeadData = () => {
    if (transferSource === "egress") return getTableSourceHeadData();
    return getTableDestinationHeadData();
  };

  const getTableSourceRowData = () => {
    const rowData = egressDataSorted.map((data) => {
      return {
        cells: [
          {
            children: (
              <Checkbox
                id={data.path}
                checked={isSourceFolderChecked(data.path)}
                onChange={handleCheckboxChange}
                ariaLabel="select folder"
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
                        handleFolderClick(data.id);
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
            children: <span>{formatDate(data.dateUpdated)}</span>,
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

  const getTableDestinationRowData = () => {
    const rowData = egressDataSorted.map((data, index) => {
      return {
        cells: [
          {
            children: (
              <div className={styles.iconButtonWrapper}>
                {data.isFolder ? (
                  <>
                    <FolderIcon />
                    <LinkButton
                      type="button"
                      onClick={() => {
                        handleFolderClick(data.id);
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
                {data.filesize ? formatFileSize(data.filesize) : "--"}
              </span>
            ),
          },
          {
            children: data.isFolder ? (
              <div>
                <DropdownButton
                  name="Actions"
                  dropDownItems={getDestinationDropdownItems(data.path)}
                  callBackFn={handleTransferAction}
                  ariaLabel="transfer actions dropdown"
                  dataTestId={`transfer-actions-dropdown-${index}`}
                  showLastItemSeparator={true}
                />
              </div>
            ) : (
              <div />
            ),
          },
        ],
      };
    });
    if (!selectedSourceLength) {
      rowData.forEach((row) => row.cells.pop());
    }
    return rowData;
  };
  const getTableRowData = () => {
    if (transferSource === "egress") return getTableSourceRowData();

    return getTableDestinationRowData();
  };

  const getDestinationDropdownItems = (id: string) => {
    return [
      {
        id: `${id}:copy`,
        label: "Copy",
        ariaLabel: "copy",
        disabled: false,
      },
    ];
  };

  const handleTransferAction = (id: string) => {
    const { actionData } = getActionDataFromId(id);
    handleSelectedActionType({
      destinationFolder: {
        path: actionData,
        name: getFolderNameFromPath(actionData),
        sourceType: "netapp",
      },
      actionType: "copy",
    });
  };

  const getInsetElement = () => {
    const curentFolder = egressPathFolders[egressPathFolders.length - 1];
    if (egressPathFolders.length === 1) return <></>;
    return (
      <InsetText data-testid="egress-inset-text">
        Transfer to {curentFolder.folderName}
        <LinkButton
          type="button"
          onClick={() => {
            handleTransferAction(`${curentFolder.folderPath}:copy`);
          }}
        >
          Copy
        </LinkButton>{" "}
      </InsetText>
    );
  };

  const hideFirstColumn = useMemo(() => {
    if (transferSource !== "egress") {
      return false;
    }
    if (!egressFolderData.length) return true;
    //below condition is to check for the root egress folder
    return egressFolderData?.[0]?.path === `${egressFolderData?.[0]?.name}/`;
  }, [transferSource, egressFolderData]);

  return (
    <div className={hideFirstColumn ? styles.hideFirstColumn : ""}>
      <FolderNavigationTable
        tableName="egress"
        folders={egressPathFolders}
        loaderText="Loading folders from Egress"
        folderResultsStatus={egressDataStatus}
        folderResultsLength={egressDataSorted.length}
        handleFolderPathClick={handleFolderPathClick}
        getTableRowData={getTableRowData}
        getTableHeadData={getTableHeadData}
        handleTableSort={handleTableSort}
        getInsetElement={getInsetElement}
        showInsetElement={!!selectedSourceLength && transferSource === "netapp"}
      />
    </div>
  );
};

export default EgressFolderContainer;
