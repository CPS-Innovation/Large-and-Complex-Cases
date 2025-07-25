import { useEffect } from "react";
import { useFocusTrap } from "../../common/hooks/useFocusTrap";
import { useLastFocus } from "../../common/hooks/useLastFocus";
import classes from "./Modal.module.scss";

type Props = {
  children: React.ReactElement;
  isVisible: boolean | undefined;
  type?: "data" | "alert" | "";
  ariaLabel: string;
  ariaDescription: string;
  handleClose: () => void;
  className?: string;
  defaultLastFocusId?: string;
};

export const Modal: React.FC<Props> = ({
  isVisible,
  children,
  ariaLabel,
  ariaDescription,
  type = "data",
  handleClose,
  className,
  defaultLastFocusId,
}) => {
  const htmlElement = document.getElementsByTagName("html")[0];
  if (isVisible) {
    htmlElement.classList.add(classes.stopHtmlScroll);
  } else {
    // We need to reenable the scrolling the behaviour for the window
    //  if we are hiding the modal. But see following comment....
    htmlElement.classList.remove(classes.stopHtmlScroll);
  }
  useLastFocus(defaultLastFocusId);
  useFocusTrap();
  useEffect(() => {
    // ... we also need to make sure the window scroll is reenabled if
    //  we are being unmounted before the the isVisible flag is seen to be
    //  false.
    return () => htmlElement.classList.remove(classes.stopHtmlScroll);
  }, [htmlElement.classList]);

  if (!isVisible) {
    return null;
  }

  return (
    <>
      <div
        className={classes.backDrop}
        role="presentation"
        onClick={handleClose}
      />
      <div
        id={"modal"}
        data-testid="div-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-label"
        aria-describedby="modal-description"
        className={
          type === "data"
            ? `${classes.modalContent} ${classes.modalContentData} ${className}`
            : `${classes.modalContent} ${className}`
        }
      >
        <span id="modal-label" className={classes.modalLabel}>
          {ariaLabel}
        </span>
        <span id="modal-description" className={classes.modalDescription}>
          {ariaDescription}
        </span>
        <div
          role="presentation"
          onKeyDown={(e: React.KeyboardEvent<HTMLDivElement>) => {
            if (e.code === "Escape") {
              handleClose();
            }
          }}
        >
          <div>{children}</div>
        </div>
      </div>
    </>
  );
};
