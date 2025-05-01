import { useState } from "react";
import { Tabs } from "../common/tabs/Tabs";
import { TabId } from "../../common/types/CaseManagement";

const CaseManagementPage = () => {
  const [activeTabId, setActiveId] = useState<TabId>("transfer-materials");
  const handleTabSelection = (tabId: TabId) => {
    setActiveId(tabId);
  };

  const items: { id: TabId; label: string }[] = [
    { id: "transfer-materials", label: "Transfer materials" },
    { id: "manage-materials", label: "Manage materials" },
  ];
  return (
    <div className="govuk-width-container">
      <h1>Thunderstruck</h1>

      <Tabs
        items={items.map((item) => ({
          id: item.id,
          label: item.label,
          panel: {
            children: (
              <div>
                <h2>Transfer Material</h2>
              </div>
            ),
          },
        }))}
        title="Contents"
        activeTabId={activeTabId}
        handleTabSelection={handleTabSelection}
      />
    </div>
  );
};

export default CaseManagementPage;
