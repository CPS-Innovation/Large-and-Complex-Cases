import { useState } from "react";
import { Tabs } from "../common/tabs/Tabs";
import { TabId } from "../../common/types/CaseManagement";
import { ItemProps } from "../common/tabs/types";
import TransferMaterialsPage from "./transfer-materials";

const CaseManagementPage = () => {
  const [activeTabId, setActiveId] = useState<TabId>("transfer-materials");
  const handleTabSelection = (tabId: TabId) => {
    setActiveId(tabId);
  };

  const items: ItemProps<TabId>[] = [
    {
      id: "transfer-materials",
      label: "Transfer materials",
      panel: { children: <TransferMaterialsPage /> },
    },
    {
      id: "manage-materials",
      label: "Manage materials",
      panel: { children: <div>manage materials</div> },
    },
  ];
  return (
    <div className="govuk-width-container">
      <h1>Thunderstruck</h1>

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
