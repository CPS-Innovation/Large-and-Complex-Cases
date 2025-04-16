import { Button, SortableTable, Tag } from "../govuk";
import { UseApiResult } from "../../common/hooks/useApi";
import { NetAppFolderData } from "../../common/types/NetAppFolderData";
type FolderActions = "connect";
type FolderNavigationProps = {
  folderData: UseApiResult<NetAppFolderData>;
  handleGetFolderContent: (folderId: string) => void;
  handleFolderAction: (actionType: FolderActions) => void;
};

const FolderNavigation: React.FC<FolderNavigationProps> = () => {
  const getTableRowData = () => {
    return egressSearchResultsData.map((data) => {
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
            children: data.caseId ? (
              <Tag gdsTagColour="green" className={styles.statusTag}>
                Connected
              </Tag>
            ) : (
              <Tag gdsTagColour="grey" className={styles.statusTag}>
                Inactive
              </Tag>
            ),
          },
          {
            children: formatDate(data.dateCreated),
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
  return (
    <div>
      <SortableTable
        head={[
          {
            children: "Operation or defendant surname",
            sortable: true,
            sortName: "workspace-name",
          },
          {
            children: "Status",
            sortable: true,
            sortName: "status",
          },

          {
            children: "Date created",
            sortable: true,
            sortName: "date-created",
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

export default FolderNavigation;
