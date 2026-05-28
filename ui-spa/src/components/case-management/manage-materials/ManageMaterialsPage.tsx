import { useState, useMemo, useEffect, useRef } from "react";
import { useApi } from "../../../common/hooks/useApi";
import {
  getNetAppFolders,
  initiateBatchCopy,
  initiateBatchMove,
} from "../../../apis/gateway-api";
import { mapToNetAppFolderData } from "../../../common/utils/mapToNetAppFolderData";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import { getFileNameFromPath } from "../../../common/utils/getFileNameFromPath";
import { pollTransferStatus } from "../../../common/utils/pollTransferStatus";
import { pollActiveManageMaterialsOperations } from "../../../common/utils/pollActiveManageMaterialsOperations";
import { ApiError } from "../../../common/errors/ApiError";
import type { TransferStatusResponse, ManageMaterialsOperation } from "../../../schemas";
import { SortableTable, LinkButton, NotificationBanner } from "../../govuk";
import { Spinner } from "../../common/Spinner";
import FolderPath, { type Folder } from "../../common/FolderPath";
import Checkbox from "../../common/Checkbox";
import { DropdownButton } from "../../common/DropdownButton";
import FolderIcon from "../../svgs/folder.svg?react";
import FileIcon from "../../svgs/file.svg?react";
import ManageMaterialsDestinationPicker from "./ManageMaterialsDestinationPicker";
import styles from "./ManageMaterialsPage.module.scss";

type PageView =
  | { type: "browse" }
  | {
      type: "pick-destination";
      action: "copy" | "move";
      conflictError: string | null;
    }
  | { type: "in-progress"; action: "copy" | "move"; transferId: string }
  | { type: "completed"; action: "copy" | "move" };

type ManageMaterialsPageProps = {
  caseId: string;
  netAppPath: string;
  operationName: string;
  isTabActive: boolean;
  initialActiveOps: ManageMaterialsOperation[];
};

const parsePaths = (jsonStr: string | null | undefined): string[] => {
  if (!jsonStr) return [];
  try {
    const parsed = JSON.parse(jsonStr);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
};

const pathsOverlap = (a: string, b: string): boolean => {
  const la = a.toLowerCase();
  const lb = b.toLowerCase();
  return la.startsWith(lb) || lb.startsWith(la);
};

const collectLockedPaths = (ops: ManageMaterialsOperation[]): string[] =>
  ops.flatMap((op) => [
    ...parsePaths(op.sourcePaths),
    ...parsePaths(op.destinationPaths),
  ]);

const ManageMaterialsPage: React.FC<ManageMaterialsPageProps> = ({
  caseId,
  netAppPath,
  operationName,
  isTabActive,
  initialActiveOps,
}) => {
  const [view, setView] = useState<PageView>({ type: "browse" });
  const [currentFolderPath, setCurrentFolderPath] = useState(netAppPath);
  const [selectedItems, setSelectedItems] = useState<string[]>([]);
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();
  const [activeOps, setActiveOps] = useState<ManageMaterialsOperation[]>(initialActiveOps);
  const [operationError, setOperationError] = useState<string | null>(null);
  const [apiRequestError, setApiRequestError] = useState<Error | null>(null);

  const unmounting = useRef(false);

  useEffect(() => {
    unmounting.current = false;
    return () => {
      unmounting.current = true;
    };
  }, []);

  useEffect(() => {
    if (apiRequestError) throw apiRequestError;
  }, [apiRequestError]);

  const {
    refetch,
    status,
    data: netAppData,
  } = useApi(getNetAppFolders, [currentFolderPath], false);

  useEffect(() => {
    if (!isTabActive) return;

    refetch();

    // Only poll if the case metadata told us there are active operations.
    // The poll stops automatically when the response comes back empty.
    if (activeOps.length === 0) return;

    let stopped = false;

    const handleActiveOps = (ops: ManageMaterialsOperation[]): boolean => {
      setActiveOps(ops);
      if (ops.length > 0) {
        const locked = collectLockedPaths(ops);
        setSelectedItems((prev) =>
          prev.filter((path) => !locked.some((lp) => pathsOverlap(path, lp))),
        );
      }
      return ops.length > 0;
    };

    pollActiveManageMaterialsOperations(
      caseId,
      () => stopped || unmounting.current,
      handleActiveOps,
    );

    return () => {
      stopped = true;
    };
  // activeOps.length is intentionally excluded — we only want this to re-run
  // when the tab activates, not every time the poll updates the ops list.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isTabActive, refetch, caseId]);

  useEffect(() => {
    if (isTabActive) {
      refetch();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentFolderPath]);

  const folderData = useMemo(
    () => (netAppData ? mapToNetAppFolderData(netAppData) : []),
    [netAppData],
  );

  const sortedFolderData = useMemo(() => {
    if (!sortValues) return folderData;
    return [...folderData].sort((a, b) => {
      const nameA = a.isFolder
        ? getFolderNameFromPath(a.path)
        : getFileNameFromPath(a.path);
      const nameB = b.isFolder
        ? getFolderNameFromPath(b.path)
        : getFileNameFromPath(b.path);
      const comparison = nameA.localeCompare(nameB);
      return sortValues.type === "ascending" ? comparison : -comparison;
    });
  }, [folderData, sortValues]);

  const lockedPaths = useMemo(() => collectLockedPaths(activeOps), [activeOps]);

  const isLocked = (path: string) =>
    lockedPaths.some((lp) => pathsOverlap(path, lp));

  const lockedItemCount = useMemo(
    () => sortedFolderData.filter((item) => isLocked(item.path)).length,
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [sortedFolderData, lockedPaths],
  );

  const breadcrumbs = useMemo((): Folder[] => {
    const replacedString = currentFolderPath.replace(netAppPath, "");
    const parts = replacedString.split("/").filter(Boolean);
    const subFolders = parts.map((folderName, index) => ({
      folderName,
      folderPath: `${netAppPath}${parts.slice(0, index + 1).join("/")}/`,
    }));
    return [
      {
        folderName: getFolderNameFromPath(netAppPath),
        folderPath: netAppPath,
      },
      ...subFolders,
    ];
  }, [currentFolderPath, netAppPath]);

  const handleFolderClick = (path: string) => {
    setCurrentFolderPath(path);
    setSelectedItems([]);
  };

  const handleFolderPathClick = (folderPath: string) => {
    const index = breadcrumbs.findIndex((b) => b.folderPath === folderPath);
    if (index !== -1) {
      setCurrentFolderPath(folderPath);
      setSelectedItems([]);
    }
  };

  const handleCheckboxChange = (id: string, checked: boolean) => {
    if (id === "select-all") {
      setSelectedItems(
        checked
          ? sortedFolderData
              .filter((item) => !isLocked(item.path))
              .map((item) => item.path)
          : [],
      );
      return;
    }
    if (isLocked(id)) return;
    setSelectedItems((prev) =>
      checked ? [...prev, id] : prev.filter((item) => item !== id),
    );
  };

  const isChecked = (id: string) => {
    if (id === "select-all") {
      const selectable = sortedFolderData.filter(
        (item) => !isLocked(item.path),
      );
      return (
        selectable.length > 0 &&
        selectable.every((item) => selectedItems.includes(item.path))
      );
    }
    return selectedItems.includes(id);
  };

  const handleAction = (actionId: string) => {
    if (actionId !== "copy" && actionId !== "move") return;
    setOperationError(null);
    setView({ type: "pick-destination", action: actionId, conflictError: null });
  };

  const handleDestinationConfirm = async (
    destinationPath: string,
  ) => {
    if (view.type !== "pick-destination") return;
    const currentAction = view.action;

    const operations = sortedFolderData
      .filter((item) => selectedItems.includes(item.path))
      .map((item) => ({
        type: item.isFolder ? ("Folder" as const) : ("Material" as const),
        sourcePath: item.path,
      }));

    const payload = {
      caseId: parseInt(caseId),
      destinationPrefix: destinationPath,
      operations,
    };

    try {
      const response =
        currentAction === "copy"
          ? await initiateBatchCopy(payload)
          : await initiateBatchMove(payload);

      setView({
        type: "in-progress",
        action: currentAction,
        transferId: response.id,
      });

      const handleStatusResponse = (
        statusResponse: TransferStatusResponse,
      ) => {
        if (statusResponse.status === "Completed") {
          setView({ type: "completed", action: currentAction });
          setSelectedItems([]);
          refetch();
        } else if (
          statusResponse.status === "PartiallyCompleted" ||
          statusResponse.status === "Failed"
        ) {
          setView({ type: "browse" });
          setOperationError(
            `The ${currentAction} operation completed with errors. Please check the activity log.`,
          );
          setSelectedItems([]);
          refetch();
        }
      };

      pollTransferStatus(
        response.id,
        () => unmounting.current,
        handleStatusResponse,
        (error) => setApiRequestError(error),
      );
    } catch (error) {
      if (error instanceof ApiError && error.code === 409) {
        setView({
          type: "pick-destination",
          action: currentAction,
          conflictError:
            error.customMessage ??
            "A conflicting operation is already in progress for this case. Please try again later.",
        });
      } else {
        throw error;
      }
    }
  };

  const handleDestinationCancel = () => {
    setView({ type: "browse" });
  };

  const handleReturnToBrowse = () => {
    setView({ type: "browse" });
  };

  const handleTableSort = (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => {
    setSortValues({ name: sortName, type: sortType });
  };

  const tableHead = [
    {
      children: (
        <div className={styles.nameCell}>
          {/* eslint-disable-next-line jsx-a11y/no-static-element-interactions, jsx-a11y/click-events-have-key-events */}
          <div onClick={(e) => e.stopPropagation()}>
            <Checkbox
              id="select-all"
              checked={isChecked("select-all")}
              onChange={handleCheckboxChange}
              ariaLabel="Select all files and folders"
            />
          </div>
          <span>File or folder</span>
        </div>
      ),
      sortable: true,
      sortName: "name",
    },
  ];

  const tableRows = sortedFolderData.map((item) => ({
    cells: [
      {
        children: (
          <div className={styles.nameCell}>
            <Checkbox
              id={item.path}
              checked={isChecked(item.path)}
              onChange={handleCheckboxChange}
              disabled={isLocked(item.path)}
              ariaLabel={
                item.isFolder
                  ? `Select folder ${getFolderNameFromPath(item.path)}`
                  : `Select file ${getFileNameFromPath(item.path)}`
              }
            />
            {item.isFolder ? (
              <>
                <FolderIcon aria-hidden="true" />
                <LinkButton
                  type="button"
                  onClick={() => handleFolderClick(item.path)}
                >
                  {getFolderNameFromPath(item.path)}
                </LinkButton>
              </>
            ) : (
              <>
                <FileIcon aria-hidden="true" />
                <span>{getFileNameFromPath(item.path)}</span>
              </>
            )}
          </div>
        ),
      },
    ],
  }));

  if (!isTabActive) return <></>;

  if (view.type === "pick-destination") {
    return (
      <ManageMaterialsDestinationPicker
        netAppPath={netAppPath}
        operationName={operationName}
        action={view.action}
        selectedCount={selectedItems.length}
        conflictError={view.conflictError}
        onConfirm={handleDestinationConfirm}
        onCancel={handleDestinationCancel}
      />
    );
  }

  if (view.type === "in-progress") {
    return (
      <div className={styles.pageWrapper}>
        <h2 className="govuk-heading-m">Manage materials on the shared drive</h2>
        <div className={styles.spinnerWrapper}>
          <Spinner data-testid="manage-materials-progress-spinner" diameterPx={50} />
          <div className={styles.spinnerText} aria-live="polite">
            <span>
              Completing {view.action} from{" "}
              <b>Shared Drive to Shared Drive...</b>
            </span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.pageWrapper}>
      <h2 className="govuk-heading-m">Manage materials on the shared drive</h2>
      <p className="govuk-body">
        You can add, rename or remove materials and folders.
      </p>

      {view.type === "completed" && (
        <div className={styles.successBanner}>
          <NotificationBanner type="success">
            <b>
              Files {view.action === "copy" ? "copied" : "moved"} successfully
            </b>
          </NotificationBanner>
        </div>
      )}

      {operationError && (
        <NotificationBanner type="important">
          <p className="govuk-body">{operationError}</p>
        </NotificationBanner>
      )}

      {activeOps.length > 0 && (
        <NotificationBanner type="important">
          <p className="govuk-body">
            A copy or move operation is in progress for this case.
            {lockedItemCount > 0 && (
              <>
                {" "}
                {lockedItemCount === 1
                  ? "1 item in this folder is"
                  : `${lockedItemCount} items in this folder are`}{" "}
                locked and cannot be selected.
              </>
            )}
          </p>
        </NotificationBanner>
      )}

      <div className={styles.toolbar}>
        <DropdownButton
          name="Actions on selection"
          disabled={selectedItems.length === 0}
          dropDownItems={[
            {
              id: "copy",
              label: "Copy",
              ariaLabel: "Copy selected items",
              disabled: false,
            },
            {
              id: "move",
              label: "Move",
              ariaLabel: "Move selected items",
              disabled: false,
            },
          ]}
          callBackFn={handleAction}
          ariaLabel="Actions on selection"
          dataTestId="manage-materials-actions-dropdown"
        />
      </div>

      <FolderPath
        folders={breadcrumbs}
        disabled={status === "loading"}
        handleFolderPathClick={handleFolderPathClick}
      />

      {status === "loading" && (
        <div className={styles.spinnerWrapper}>
          <Spinner data-testid="manage-materials-loader" diameterPx={50} />
          <div className={styles.spinnerText} aria-live="polite">
            Loading folders from Shared Drive
          </div>
        </div>
      )}

      {status === "succeeded" && (
        <>
          <div aria-live="polite" className="govuk-visually-hidden">
            {sortedFolderData.length
              ? "Files and folders loaded successfully"
              : "There are no documents currently in this folder"}
          </div>
          <SortableTable
            captionClassName="govuk-visually-hidden"
            caption="Shared drive files and folders table, column headers with buttons are sortable"
            head={tableHead}
            rows={tableRows}
            handleTableSort={handleTableSort}
          />
          {!sortedFolderData.length && (
            <p
              className="govuk-body"
              data-testid="manage-materials-no-documents"
            >
              There are no documents currently in this folder
            </p>
          )}
        </>
      )}

      {view.type === "completed" && (
        <LinkButton onClick={handleReturnToBrowse}>
          Return to folder view
        </LinkButton>
      )}
    </div>
  );
};

export default ManageMaterialsPage;
