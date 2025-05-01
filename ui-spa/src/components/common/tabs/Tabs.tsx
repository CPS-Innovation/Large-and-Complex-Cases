import { CommonTabsProps } from "./types";
import TabButtons from "./TabButtons";
import { TabId } from "../../../common/types/CaseManagement";
import classes from "./Tabs.module.scss";

export type TabsProps = CommonTabsProps & {
  activeTabId: string | undefined;
  handleTabSelection: (tabId: TabId) => void;
};

export const Tabs: React.FC<TabsProps> = ({
  className,
  items,
  activeTabId,
  handleTabSelection,
  ...attributes
}) => {
  const activeTabArrayPos = items.findIndex((item) => item.id === activeTabId);
  const activeTabIndex = activeTabArrayPos === -1 ? 0 : activeTabArrayPos;

  const panels = items.map((item, index) => {
    const { id: itemId, panel } = item;
    const panelId = itemId;

    const coreProps = {
      key: panelId,
      role: "tabpanel",
      tabIndex: 0,
      "data-testid": `tab-content-${itemId}`,
    };

    return (
      <div
        id={index === activeTabIndex ? "active-tab-panel" : `panel-${index}`}
        {...coreProps}
        className={`govuk-tabs__panel ${
          index !== activeTabIndex ? "govuk-tabs__panel--hidden" : ""
        }  ${classes.contentArea}`}
      >
        {panel.children}
      </div>
    );
  });

  const tabItems = items.map((item) => ({
    id: item.id,
    label: item.label,
    ariaLabel: item.label,
  }));

  return (
    <>
      <div
        data-testid="tabs"
        className={`govuk-tabs ${classes.tabs} ${className || ""} `}
        {...attributes}
      >
        <TabButtons
          items={tabItems}
          activeTabIndex={activeTabIndex}
          handleTabSelection={handleTabSelection}
        />
        {panels}
      </div>
    </>
  );
};
