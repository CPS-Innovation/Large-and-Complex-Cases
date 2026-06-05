import { Button, LinkButton } from "../../govuk";
import styles from "./TransferControls.module.scss";

type TransferControlsProps = {
  transferDirection: "egressToSharedDrive" | "sharedDriveToEgress";
  toggleTransferDirection: () => void;
  onCopy?: () => void;
  onMove?: () => void;
};

const TransferControls = ({
  onCopy,
  onMove,
  transferDirection,
  toggleTransferDirection,
}: TransferControlsProps) => {
  return (
    <div className={styles.transferControls}>
      <Button onClick={onCopy}>Copy selected</Button>
      {transferDirection === "egressToSharedDrive" && (
        <Button onClick={onMove}>Move selected</Button>
      )}
      <LinkButton onClick={toggleTransferDirection}>
        {transferDirection === "egressToSharedDrive"
          ? "View Shared Drive"
          : "View Egress"}
      </LinkButton>
    </div>
  );
};

export default TransferControls;
