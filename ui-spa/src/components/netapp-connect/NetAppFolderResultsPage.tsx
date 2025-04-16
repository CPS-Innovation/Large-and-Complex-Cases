import { useMemo, useState } from "react";
import { Button, SortableTable, InsetText, BackLink } from "../govuk";
import { UseApiResult } from "../../common/hooks/useApi";
import { NetAppFolderData } from "../../common/types/NetAppFolderData";
import { sortByStringProperty } from "../../common/utils/sortUtils";

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
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();
  const netappFolderData = useMemo(() => {
    if (!netAppFolderApiResults?.data) return [];
    if (sortValues?.name === "workspace-name")
      return sortByStringProperty(
        netAppFolderApiResults.data,
        "name",
        sortValues.type,
      );

    return netAppFolderApiResults.data;
  }, [netAppFolderApiResults]);

  console.log("netappFolderData>>>", netappFolderData);
  const getTableRowData = () => {
    return netappFolderData.map((data) => {
      return {
        cells: [
          {
            children: (
              <div>
                <b>{data.name}</b>
              </div>
            ),
          },

          {
            children: (
              <Button
                className="govuk-button--secondary"
                name="secondary"
                onClick={() => handleConnect(data.id)}
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
      <SortableTable
        head={[
          {
            children: "Folder name",
            sortable: true,
            sortName: "workspace-name",
          },

          {
            children: "",
            sortable: false,
          },
        ]}
        rows={getTableRowData()}
        handleTableSort={handleTableSort}
      />
    </div>
  );
};

export default NetAppFolderResultsPage;
