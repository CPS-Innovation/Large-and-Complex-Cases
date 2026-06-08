import { Button, LinkButton } from "../../govuk";
import styles from "./TransferControls.module.scss";

type TransferControlsProps = {
  transferSource: "egress" | "sharedDrive";
  toggleTransferDirection: () => void;
  onCopy?: () => void;
  onMove?: () => void;
};

const TransferControls = ({
  onCopy,
  onMove,
  transferSource,
  toggleTransferDirection,
}: TransferControlsProps) => {
  return (
    <div className={styles.transferControls}>
      <Button className="govuk-button--secondary" onClick={onCopy}>
        Copy selected
      </Button>
      {transferSource === "egress" && (
        <Button className="govuk-button--secondary" onClick={onMove}>
          Move selected
        </Button>
      )}
      <LinkButton onClick={toggleTransferDirection}>
        {transferSource === "egress" ? "View Shared Drive" : "View Egress"}
      </LinkButton>
    </div>
  );
};

export default TransferControls;
