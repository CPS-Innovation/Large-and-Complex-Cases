import { useMemo, useState } from "react";
import { Button, InsetText, BackLink, LinkButton } from "../govuk";
import { UseApiResult } from "../../common/hooks/useApi";
import { ConnectNetAppFolderData } from "../../common/types/ConnectNetAppFolderData";
import { sortByStringProperty } from "../../common/utils/sortUtils";
import { getFolderNameFromPath } from "../../common/utils/getFolderNameFromPath";
import FolderNavigationTable from "../common/FolderNavigationTable";
import FolderIcon from "../../components/svgs/folder.svg?react";
import styles from "./netAppFolderResultsPage.module.scss";

type NetAppFolderResultsPageProps = {
  backLinkUrl: string;
  rootFolderPath: string;
  netAppFolderApiResults: UseApiResult<ConnectNetAppFolderData>;
  handleGetFolderContent: (folderId: string) => void;
  handleConnectFolder: (id: string) => void;
};

const NetAppFolderResultsPage: React.FC<NetAppFolderResultsPageProps> = ({
  backLinkUrl,
  rootFolderPath,
  netAppFolderApiResults,
  handleConnectFolder,
  handleGetFolderContent,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

  const netappFolderData = useMemo(() => {
    if (!netAppFolderApiResults?.data?.folders) return [];
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(
        netAppFolderApiResults.data.folders,
        "path",
        sortValues.type,
      );

    return netAppFolderApiResults.data.folders;
  }, [netAppFolderApiResults, sortValues]);

  const folders = useMemo(() => {
    const parts = rootFolderPath.split("/").filter(Boolean);

    const result = parts.map((folderName, index) => ({
      folderName,
      folderPath: `${parts.slice(0, index + 1).join("/")}/`,
    }));
    const withHome = [{ folderName: "Home", folderPath: "" }, ...result];
    return withHome;
  }, [rootFolderPath]);

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
                    handleGetFolderContent(data.path);
                  }}
                >
                  {getFolderNameFromPath(data.path)}
                </LinkButton>
              </div>
            ),
          },

          {
            children: (
              <Button
                className="govuk-button--secondary"
                name="secondary"
                onClick={() => handleConnect(data.path)}
                disabled={!!data.caseId}
              >
                Connect folder
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
    <div className={`govuk-width-container ${styles.mainContainer}`}>
      <BackLink to={backLinkUrl}>Back</BackLink>
      <h1 className="govuk-heading-xl govuk-!-margin-bottom-0">
        Select a network shared drive folder to link to the case
      </h1>
      <InsetText>
        <p>Select a folder from the list to link it to this case.</p>
        <p>
          If the folder you need is not listed, check that you have the correct
          permissions or contact the product team for support.
        </p>
      </InsetText>

      <div className={"govuk-grid-column-two-thirds"}>
        <FolderNavigationTable
          tableName={"netapp"}
          folders={folders}
          loaderText="Loading folders from Network Shared Drive"
          folderResultsStatus={netAppFolderApiResults.status}
          folderResultsLength={netappFolderData.length}
          handleFolderPathClick={handleFolderPathClick}
          getTableRowData={getTableRowData}
          getTableHeadData={getTableHeadData}
          handleTableSort={handleTableSort}
        />
      </div>
    </div>
  );
};

export default NetAppFolderResultsPage;
