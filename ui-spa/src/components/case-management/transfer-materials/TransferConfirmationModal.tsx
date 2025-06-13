import { Checkboxes, Button, LinkButton } from "../../govuk";
import { useState } from "react";
import { Modal } from "../../common/Modal";
import { TransferAction } from "../../../common/types/TransferAction";
import styles from "./transferConfirmationModal.module.scss";

type TransferConfirmationModalProps = {
  transferAction: TransferAction;
  groupedData: { folders: string[]; files: string[] };
  handleCloseModal: () => void;
  handleContinue: () => void;
};

const TransferConfirmationModal: React.FC<TransferConfirmationModalProps> = ({
  transferAction,
  groupedData,
  handleCloseModal,
  handleContinue,
}) => {
  const [acceptedConfirmation, setAcceptedConfirmation] = useState(false);

  const getConfirmationText = () => {
    let folderText,
      fileText = "";
    if (groupedData.folders.length)
      folderText =
        groupedData.folders.length > 1
          ? `${groupedData.folders.length} folders`
          : "1 folder";
    if (groupedData.files.length)
      fileText =
        groupedData.files.length > 1
          ? `${groupedData.files.length} files`
          : "1 file";
    if (folderText && fileText)
      return (
        <div>
          I confirm I want to{" "}
          <b>
            {transferAction.actionType} {folderText}
          </b>{" "}
          and <b>{fileText}</b> to{" "}
          <b>{transferAction.destinationFolder.name}</b>
        </div>
      );
    return (
      <div>
        I confirm I want to{" "}
        <b>
          {transferAction.actionType} {folderText}
          {fileText}
        </b>{" "}
        to <b>{transferAction.destinationFolder.name}</b>
      </div>
    );
  };
  return (
    <Modal
      isVisible={true}
      className={styles.transferConfirmationModal}
      handleClose={handleCloseModal}
      type="alert"
      ariaLabel="Transfer confirmation alert modal"
      ariaDescription={` ${transferAction.actionType === "copy" ? "Copy" : "Move"} files to:{" "}
          ${transferAction.destinationFolder.name}`}
    >
      <div>
        <div className={styles.modalHeader}>
          {transferAction.actionType === "copy" ? "Copy" : "Move"} files to:{" "}
          {transferAction.destinationFolder.name}
        </div>
        <div className={styles.modalContent}>
          <Checkboxes
            className="govuk-checkboxes--small"
            items={[
              {
                checked: acceptedConfirmation,
                children: getConfirmationText(),
              },
            ]}
            name="confirmation checkbox"
            onChange={() => setAcceptedConfirmation(!acceptedConfirmation)}
          />
          <div className={styles.modalButtonWrapper}>
            <Button onClick={handleContinue} disabled={!acceptedConfirmation}>
              Continue
            </Button>
            <LinkButton onClick={handleCloseModal}>Cancel </LinkButton>
          </div>
        </div>
      </div>
    </Modal>
  );
};

export default TransferConfirmationModal;
