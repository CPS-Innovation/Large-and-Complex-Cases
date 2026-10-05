import { useMemo, useState } from "react";
import { Button, InsetText, BackLink, LinkButton } from "../govuk";
import { type ConnectNetAppFolderData } from "../../schemas";
import { sortByStringProperty } from "../../common/utils/sortUtils";
import { getFolderNameFromPath } from "../../common/utils/getFolderNameFromPath";
import FolderNavigationTable from "../common/FolderNavigationTable";
import FolderIcon from "../../components/svgs/folder.svg?react";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import styles from "./NetAppFolderResultsPage.module.scss";

type NetAppFolderResultsPageProps = {
  backLinkUrl: string;
  rootFolderPath: string;
  netAppFolderResults: ConnectNetAppFolderData;
  isNetAppFolderResultsLoading: boolean;
  accessibleRoots?: string[] | null;
  handleGetFolderContent: (folderId: string) => void;
  handleConnectFolder: (id: string) => void;
};

const NetAppFolderResultsPage: React.FC<NetAppFolderResultsPageProps> = ({
  backLinkUrl,
  rootFolderPath,
  netAppFolderResults,
  isNetAppFolderResultsLoading,
  accessibleRoots,
  handleConnectFolder,
  handleGetFolderContent,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

  const netappFolderData = useMemo(() => {
    if (!netAppFolderResults?.folders) return [];
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(
        netAppFolderResults.folders,
        "folderPath",
        sortValues.type,
      );

    return netAppFolderResults.folders;
  }, [netAppFolderResults, sortValues]);

  const folders = useMemo(() => {
    const parts = rootFolderPath.split("/").filter(Boolean);

    // When the user's access is scoped to an entry prefix, the segments inside that prefix are
    // shown for context but are not listable, so only the accessible root onwards is clickable.
    // Home stays clickable because re-listing the root returns the entry prefixes again.
    const accessibleRoot = accessibleRoots?.find((root) =>
      rootFolderPath.startsWith(root),
    );

    const result = parts.map((folderName, index) => {
      const folderPath = `${parts.slice(0, index + 1).join("/")}/`;
      return {
        folderName,
        folderPath,
        isNavigable:
          !accessibleRoot || folderPath.length >= accessibleRoot.length,
      };
    });
    const withHome = [{ folderName: "Home", folderPath: "" }, ...result];
    return withHome;
  }, [rootFolderPath, accessibleRoots]);

  const getTableRowData = () => {
    return netappFolderData.map((data) => {
      return {
        cells: [
          {
            children: (
              <div className={styles.folderWrapper}>
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

          {
            children: (
              <Button
                className="govuk-button--secondary"
                name="secondary"
                onClick={() => handleConnect(data.folderPath)}
                disabled={!!data.caseId}
              >
                Connect
              </Button>
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

  const handleConnect = (id: string) => {
    handleConnectFolder(id);
  };

  const handleFolderPathClick = (path: string) => {
    handleGetFolderContent(path);
  };

  return (
    <div className={styles.mainContainer}>
      <BackLink to={backLinkUrl}>Back</BackLink>
      <PageContentWrapper>
        <h1 className="govuk-heading-xl govuk-!-margin-bottom-0">
          Link a Shared Drive folder to the case
        </h1>
        <InsetText>
          {netAppFolderResults.isRestrictedRoot ? (
            <p data-testid="restricted-root-text">
              You have access to a restricted area of this bucket. Select one of
              the available folders below to continue.
            </p>
          ) : (
            <p>
              If the folder you need is not listed, check that you have the
              correct permissions or contact the product team for support.
            </p>
          )}
        </InsetText>

        <div className={styles.tableContainer}>
          <FolderNavigationTable
            caption="shared drive folders table, column headers with buttons are sortable"
            tableName={"netapp"}
            folders={folders}
            loaderText="Loading folders from Network Shared Drive"
            isLoading={isNetAppFolderResultsLoading}
            folderResultsLength={netappFolderData.length}
            handleFolderPathClick={handleFolderPathClick}
            getTableRowData={getTableRowData}
            getTableHeadData={getTableHeadData}
            handleTableSort={handleTableSort}
          />
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default NetAppFolderResultsPage;
