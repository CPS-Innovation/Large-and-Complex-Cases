import { useMemo, useState } from "react";
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
  const netappFolderData = useMemo(() => {
    if (!netAppFolderApiResults?.data) return [];
    if (sortValues?.name === "folder-name")
      return sortByStringProperty(
        netAppFolderApiResults.data,
        "folderPath",
        sortValues.type,
      );

    return netAppFolderApiResults.data;
  }, [netAppFolderApiResults, sortValues]);

  console.log("netappFolderData>>>", netappFolderData);
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
      <div className={styles.results}>
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
        {netAppFolderApiResults.status === "succeeded" && (
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
        )}
      </div>
    </div>
  );
};

export default NetAppFolderResultsPage;
