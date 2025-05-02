import { CommonTabsProps } from "./types";
import TabButtons from "./TabButtons";
import classes from "./Tabs.module.scss";

export type TabsProps<T extends string> = CommonTabsProps<T> & {
  activeTabId: T;
  handleTabSelection: (tabId: T) => void;
};

export const Tabs = <T extends string>({
  className,
  items,
  activeTabId,
  handleTabSelection,
  ...attributes
}: TabsProps<T>) => {
  const activeTabArrayPos = items.findIndex((item) => item.id === activeTabId);
  const activeTabIndex = activeTabArrayPos === -1 ? 0 : activeTabArrayPos;

  const panels = items.map((item, index) => {
    const { id: itemId, panel } = item;

    const coreProps = {
      role: "tabpanel",
      tabIndex: 0,
      "data-testid": `tab-content-${itemId}`,
    };

    return (
      <div
        key={itemId}
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
