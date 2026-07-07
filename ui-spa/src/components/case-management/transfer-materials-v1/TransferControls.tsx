import { Button, LinkButton } from "../../govuk";
import styles from "./TransferControls.module.scss";

type TransferControlsProps = {
  transferSource: "egress" | "netapp";
  disableControls: boolean;
  toggleTransferDirection: () => void;
  onCopy?: () => void;
  onMove?: () => void;
};

const TransferControls = ({
  onCopy,
  onMove,
  disableControls,
  transferSource,
  toggleTransferDirection,
}: TransferControlsProps) => {
  return (
    <div className={styles.transferControls} data-testid="transfer-controls">
      <Button
        className="govuk-button--secondary"
        onClick={onCopy}
        disabled={disableControls}
      >
        Copy selected
      </Button>
      {transferSource === "egress" && (
        <Button
          className="govuk-button--secondary"
          onClick={onMove}
          disabled={disableControls}
        >
          Move selected
        </Button>
      )}
      <LinkButton
        onClick={toggleTransferDirection}
        dataTestId="toggle-transfer-direction"
      >
        {transferSource === "egress" ? "View Shared Drive" : "View Egress"}
      </LinkButton>
    </div>
  );
};

export default TransferControls;
