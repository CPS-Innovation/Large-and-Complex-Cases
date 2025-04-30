import { Tabs } from "../common/tabs/Tabs";

const CaseManagementPage = () => {
  const handleTabSelection = () => {};

  const items = [
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
        activeTabId={"manage-materials"}
        handleTabSelection={handleTabSelection}
      />
    </div>
  );
};

export default CaseManagementPage;
