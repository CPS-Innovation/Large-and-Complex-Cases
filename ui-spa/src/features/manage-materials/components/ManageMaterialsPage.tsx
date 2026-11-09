import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router";
import { useQuery } from "@tanstack/react-query";
import { getNetAppFolders } from "../../../apis/gateway-api";
import { ApiError } from "../../../common/errors/ApiError";
import { mapToNetAppFolderData } from "../../../common/utils/mapToNetAppFolderData";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import { getUrlSearchParam } from "../../../common/utils/getUrlSearchParam";
import { sortByStringProperty } from "../../../common/utils/sortUtils";
import { type NetAppFileFolder } from "../../../schemas";
import MaterialsTable, { SELECT_ALL_CHECKBOX_ID } from "./MaterialsTable";
import styles from "./ManageMaterialsPage.module.scss";

type ManageMaterialsPageProps = {
  isTabActive: boolean;
  caseId: string;
  operationName: string;
  netAppPath: string;
};

const ManageMaterialsPage: React.FC<ManageMaterialsPageProps> = ({
  isTabActive,
  caseId,
  operationName,
  netAppPath,
}) => {
  const navigate = useNavigate();
  const [folderPath, setFolderPath] = useState(netAppPath);
  const [selectedPaths, setSelectedPaths] = useState<string[]>([]);
  const [sortType, setSortType] = useState<"ascending" | "descending">();

  const {
    data: netAppData,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ["manage-materials", folderPath],
    queryFn: () => getNetAppFolders(folderPath),
    retry: false,
    enabled: isTabActive && !!folderPath,
    staleTime: 0,
    gcTime: 0,
  });

  const items = useMemo(() => {
    const folderData = netAppData ? mapToNetAppFolderData(netAppData) : [];
    return sortType
      ? sortByStringProperty(folderData, "name", sortType)
      : folderData;
  }, [netAppData, sortType]);

  const breadcrumbFolders = useMemo(() => {
    const parts = folderPath.replace(netAppPath, "").split("/").filter(Boolean);
    return [
      {
        folderName: `Shared Drive: ${getFolderNameFromPath(netAppPath)}`,
        folderPath: netAppPath,
      },
      ...parts.map((folderName, index) => ({
        folderName,
        folderPath: `${netAppPath}${parts.slice(0, index + 1).join("/")}/`,
      })),
    ];
  }, [folderPath, netAppPath]);

  useEffect(() => {
    if (!isError || !error) return;
    if (error instanceof ApiError && error.code === 404) {
      navigate(
        `/case/${caseId}/case-management/shared-drive-connection-error?${getUrlSearchParam("operation-name", operationName)}`,
        { state: { isRouteValid: true } },
      );
      return;
    }
    if (error instanceof ApiError && error.code === 401) {
      navigate(
        `/case/${caseId}/case-management/connection-error?type=shareddrive`,
        { state: { isRouteValid: true } },
      );
      return;
    }
    throw error;
  }, [isError, error, caseId, operationName, navigate]);

  const handleCheckboxChange = (id: string, checked: boolean) => {
    if (id === SELECT_ALL_CHECKBOX_ID) {
      setSelectedPaths(checked ? items.map(({ path }) => path) : []);
      return;
    }
    setSelectedPaths((previous) =>
      checked ? [...previous, id] : previous.filter((path) => path !== id),
    );
  };

  const navigateToFolder = (path: string) => {
    setFolderPath(path);
    setSelectedPaths([]);
  };

  const handleFolderClick = (item: NetAppFileFolder) =>
    navigateToFolder(item.path);

  const handleTableSort = (
    _sortName: string,
    type: "ascending" | "descending",
  ) => setSortType(type);

  if (!isTabActive) return null;

  return (
    <div data-testid="manage-materials-page">
      <h2>Manage materials on the shared drive</h2>
      <p>You can add, rename or remove materials and folders.</p>
      <hr className={styles.sectionBreak} />

      <nav
        className={`govuk-breadcrumbs ${styles.breadcrumbs}`}
        aria-label="Breadcrumb"
        data-testid="manage-materials-breadcrumbs"
      >
        <ol className="govuk-breadcrumbs__list">
          {breadcrumbFolders.map((folder, index) => (
            <li
              key={folder.folderPath}
              className="govuk-breadcrumbs__list-item"
            >
              {index === breadcrumbFolders.length - 1 ? (
                folder.folderName
              ) : (
                <button
                  type="button"
                  className={`govuk-breadcrumbs__link ${styles.breadcrumbLink}`}
                  disabled={isLoading}
                  onClick={() => navigateToFolder(folder.folderPath)}
                >
                  {folder.folderName}
                </button>
              )}
            </li>
          ))}
        </ol>
      </nav>

      <MaterialsTable
        items={items}
        isLoading={isLoading}
        selectedPaths={selectedPaths}
        handleCheckboxChange={handleCheckboxChange}
        handleFolderClick={handleFolderClick}
        handleTableSort={handleTableSort}
      />
    </div>
  );
};

export default ManageMaterialsPage;
