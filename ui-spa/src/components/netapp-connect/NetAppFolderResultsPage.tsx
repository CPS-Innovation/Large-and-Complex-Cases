import { useMemo, useState, useEffect } from "react";
import {
  Button,
  SortableTable,
  InsetText,
  BackLink,
  LinkButton,
} from "../govuk";
import { Spinner } from "../common/Spinner";
import { UseApiResult } from "../../common/hooks/useApi";
import { NetAppFolderData } from "../../common/types/NetAppFolderData";
import { sortByStringProperty } from "../../common/utils/sortUtils";
import { getFolderNameFromPath } from "../../common/utils/getFolderNameFromPath";
import FolderPath from "../common/FolderPath";
import FolderIcon from "../../components/svgs/folder.svg?react";
import styles from "./netAppFolderResultsPage.module.scss";

type NetAppFolderResultsPageProps = {
  backLinkUrl: string;
  netAppFolderApiResults: UseApiResult<NetAppFolderData>;
  handleGetFolderContent: (folderId: string) => void;
  handleConnectFolder: (id: string) => void;
};

const NetAppFolderResultsPage: React.FC<NetAppFolderResultsPageProps> = ({
  backLinkUrl,
  netAppFolderApiResults,
  handleConnectFolder,
  handleGetFolderContent,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

  const [currentPath, setCurrentPath] = useState<string | null>(null);
  const netappFolderData = useMemo(() => {
    if (!netAppFolderApiResults?.data?.folders) return [];
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(
        netAppFolderApiResults.data.folders,
        "folderPath",
        sortValues.type,
      );

    return netAppFolderApiResults.data.folders;
  }, [netAppFolderApiResults, sortValues]);

  useEffect(() => {
    if (!currentPath) {
      if (netAppFolderApiResults?.data?.rootPath === "") setCurrentPath("");
      if (netAppFolderApiResults?.data?.rootPath) {
        const path = netAppFolderApiResults?.data?.rootPath.replace(
          /\/[^/]+$/,
          "",
        );
        setCurrentPath(path);
      }
    }
  }, [netAppFolderApiResults?.data?.rootPath, currentPath]);

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
                    handleFolderClickHandler(data.folderPath);
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
                Connect folder
              </Button>
            ),
          },
        ],
      };
    });
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
  const handleFolderClickHandler = (path: string) => {
    setCurrentPath(path);
    handleGetFolderContent(path);
  };
  return (
    <div className="govuk-width-container">
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
      <div className={`govuk-grid-column-two-thirds ${styles.results}`}>
        {netAppFolderApiResults.status === "loading" && (
          <div className={styles.spinnerWrapper}>
            <Spinner
              diameterPx={50}
              ariaLabel="Loading folders from Network Shared Drive"
            />
            <div className={styles.spinnerText}>
              Loading folders from Network Shared Drive
            </div>
          </div>
        )}
        <div>
          {currentPath !== null && (
            <FolderPath
              path={currentPath}
              disabled={netAppFolderApiResults.status === "loading"}
              folderClickHandler={handleFolderClickHandler}
            />
          )}
          {netAppFolderApiResults.status === "succeeded" && (
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
              {!netappFolderData.length && (
                <p>There are no documents currenlty in this folder</p>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default NetAppFolderResultsPage;
