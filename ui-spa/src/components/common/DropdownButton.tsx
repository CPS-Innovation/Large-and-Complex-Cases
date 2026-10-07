import { useState, useRef } from "react";
import FilledArrow from "../svgs/filledArrow.svg?react";
import { Button } from "../govuk";

import classes from "./DropdownButton.module.scss";
import { useGlobalDropdownClose } from "../../common/hooks/useGlobalDropdownClose";

export type DropdownButtonItem = {
  id: string;
  label: string;
  ariaLabel: string;
  disabled: boolean;
};
export type DropdownButtonProps = {
  name?: string;
  dropDownItems: DropdownButtonItem[];
  callBackFn: (id: string) => void;
  ariaLabel?: string;
  dataTestId?: string;
  disabled?: boolean;
  showLastItemSeparator?: boolean;
  icon?: React.ReactElement;
};

export const DropdownButton: React.FC<DropdownButtonProps> = ({
  dropDownItems,
  callBackFn,
  name,
  dataTestId = "dropdown-btn",
  ariaLabel = "dropdown",
  disabled = false,
  icon = <FilledArrow className={classes.icon} />,
}) => {
  const dropDownBtnRef = useRef<HTMLButtonElement | null>(null);
  const panelRef = useRef<HTMLDivElement | null>(null);
  const [buttonOpen, setButtonOpen] = useState(false);

  useGlobalDropdownClose(
    dropDownBtnRef,
    panelRef,
    setButtonOpen,
    "#dropdown-panel",
  );

  const handleBtnClick = (id: string) => {
    setButtonOpen(false);
    callBackFn(id);
  };

  return (
    <div className={classes.dropDownButtonWrapper}>
      <Button
        id={dataTestId}
        ref={dropDownBtnRef}
        aria-label={ariaLabel}
        aria-expanded={buttonOpen}
        className={`govuk-button--secondary ${buttonOpen && classes.upArrow}`}
        disabled={disabled}
        onClick={() => {
          setButtonOpen((buttonOpen) => !buttonOpen);
        }}
      >
        {name && <span className={classes.dropdownBtnName}>{name}</span>}
        {icon}
      </Button>

      {buttonOpen && (
        <div
          className={classes.panel}
          ref={panelRef}
          id="dropdown-panel"
          data-testid={`dropdown-panel`}
        >
          <ul className={classes.panelList}>
            {dropDownItems.map((item) => (
              <li key={item.id} className={classes.panelListItem}>
                <Button
                  className="govuk-button--secondary"
                  aria-label={item.ariaLabel}
                  disabled={item.disabled}
                  onClick={() => {
                    handleBtnClick(item.id);
                  }}
                >
                  {item.label}
                </Button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};
