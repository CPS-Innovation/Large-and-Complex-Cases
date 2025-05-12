import { useState } from "react";
import { Tabs } from "../common/tabs/Tabs";
import { TabId } from "../../common/types/CaseManagement";
import { ItemProps } from "../common/tabs/types";
import TransferMaterialsPage from "./transfer-materials";
import { useParams } from "react-router-dom";
import { useApi } from "../../common/hooks/useApi";
import { getCaseMetaData } from "../../apis/gateway-api";
import styles from "./index.module.scss";

const CaseManagementPage = () => {
  const { caseId } = useParams();
  const caseMetaData = useApi(getCaseMetaData, [caseId], true);

  const [activeTabId, setActiveId] = useState<TabId>("transfer-materials");
  const handleTabSelection = (tabId: TabId) => {
    setActiveId(tabId);
  };

  const items: ItemProps<TabId>[] = [
    {
      id: "transfer-materials",
      label: "Transfer materials",
      panel: {
        children: (
          <TransferMaterialsPage
            egressWorkspaceId={caseMetaData?.data?.egressWorkspaceId}
            netappFolderPath={caseMetaData?.data?.netappFolderPath}
          />
        ),
      },
    },
    {
      id: "manage-materials",
      label: "Manage materials",
      panel: { children: <div>manage materials</div> },
    },
  ];
  return (
    <div className="govuk-width-container">
      <h1 className={styles.workspaceName}>
        {caseMetaData?.data?.operationName}
      </h1>
      <div className={styles.urnText}>
        <span>{caseMetaData?.data?.urn}</span>
      </div>

      <Tabs
        items={items.map((item) => ({
          id: item.id,
          label: item.label,
          panel: item.panel,
        }))}
        title="Contents"
        activeTabId={activeTabId}
        handleTabSelection={handleTabSelection}
      />
    </div>
  );
};

export default CaseManagementPage;
