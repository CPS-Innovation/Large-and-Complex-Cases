import { useEffect, useRef } from "react";
import classes from "./Tabs.module.scss";

export type TabButtonProps = {
  items: { id: string; label: string; ariaLabel: string }[];
  activeTabIndex: number;
  handleTabSelection: (documentId: string) => void;
};

const ARROW_KEY_SHIFTS = {
  ArrowLeft: -1,
  ArrowRight: 1,
};

const TabButtons: React.FC<TabButtonProps> = ({
  items,
  activeTabIndex,
  handleTabSelection,
}) => {
  const activeTabRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    activeTabRef.current?.focus();
    activeTabRef.current?.parentElement?.scrollIntoView({
      behavior: "smooth",
      block: "nearest",
    });
  }, [activeTabIndex, items.length]);

  const handleKeyPressOnTab: React.KeyboardEventHandler<HTMLButtonElement> = (
    ev,
  ) => {
    const typedKeyCode = ev.code as keyof typeof ARROW_KEY_SHIFTS;
    const thisShift = ARROW_KEY_SHIFTS[typedKeyCode]; // -1, 1 or undefined
    if (!thisShift) {
      return;
    }
    moveToNextOrPreviousTab(thisShift);
    if (ev.code === "ArrowRight" || ev.code === "ArrowLeft") {
      ev.preventDefault();
    }
  };

  const moveToNextOrPreviousTab = (thisShift: number) => {
    const shouldNavigate =
      // must be a left or right key press command
      !!thisShift &&
      // can't go left on the first tab
      !(activeTabIndex === 0 && thisShift === -1) &&
      // can't go right on the last tab
      !(activeTabIndex === items.length - 1 && thisShift === 1);

    if (!shouldNavigate) {
      return;
    }

    const nextTabIndex = activeTabIndex + thisShift;
    const nextTabId = items[nextTabIndex].id;
    handleTabSelection(nextTabId);
  };

  if (!items.length) {
    return null;
  }
  return (
    <div
      id="document-tabs"
      className={`${classes.tabsWrapper} ${classes.contentArea}`}
    >
      <ul className={`${classes.tabsList}`} role="tablist">
        {items.map((item, index) => {
          const { id: itemId, label, ariaLabel } = item;

          return (
            <li
              className={`${
                activeTabIndex === index
                  ? classes.activeTab
                  : classes.inactiveTab
              } ${classes.tabListItem}`}
              key={itemId}
              data-testid={`tab-${index}`}
              role="presentation"
            >
              <button
                id={`tab_${index}`}
                aria-controls={
                  index === activeTabIndex
                    ? "active-tab-panel"
                    : `panel-${index}`
                }
                aria-label={ariaLabel}
                role="tab"
                className={classes.tabButton}
                data-testid={
                  index === activeTabIndex ? "tab-active" : `btn-tab-${index}`
                }
                onClick={() => {
                  if (itemId !== items[activeTabIndex].id) {
                    handleTabSelection(itemId);
                  }
                }}
                onKeyDown={handleKeyPressOnTab}
                tabIndex={index === activeTabIndex ? 0 : -1}
                ref={index === activeTabIndex ? activeTabRef : undefined}
              >
                <span className={classes.tabLabel}>{label}</span>
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
};

export default TabButtons;
