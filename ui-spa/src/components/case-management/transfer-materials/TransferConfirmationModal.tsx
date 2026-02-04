import { Checkboxes, Button, LinkButton, Details } from "../../govuk";
import { useState, useMemo } from "react";
import { Modal } from "../../common/Modal";
import { TransferAction } from "../../../common/types/TransferAction";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { getFileNameFromPath } from "../../../common/utils/getFileNameFromPath";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import styles from "./TransferConfirmationModal.module.scss";

type TransferConfirmationModalProps = {
  transferAction: TransferAction;
  groupedData: { folders: string[]; files: string[] };
  duplicateFoldersAndFiles?: { folders: string[]; files: string[] };
  handleCloseModal: () => void;
  handleContinue: () => void;
};

const TransferConfirmationModal: React.FC<TransferConfirmationModalProps> = ({
  transferAction,
  groupedData,
  duplicateFoldersAndFiles = { folders: [], files: [] },
  handleCloseModal,
  handleContinue,
}) => {
  const [acceptedConfirmation, setAcceptedConfirmation] = useState(false);

  const duplicateCount = useMemo(
    () =>
      duplicateFoldersAndFiles.folders.length +
      duplicateFoldersAndFiles.files.length,
    [duplicateFoldersAndFiles],
  );

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
          I want to{" "}
          <b>
            {transferAction.actionType} {folderText}
          </b>{" "}
          and <b>{fileText}</b> to{" "}
          <b>{transferAction.destinationFolder.name}</b>
        </div>
      );
    return (
      <div>
        I want to{" "}
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
      ariaDescription="Confirm"
    >
      <div>
        <div className={styles.modalHeader}>Confirm</div>
        <div className={styles.modalContent}>
          {duplicateCount > 0 && (
            <div className={styles.duplicateWarning}>
              <p className="govuk-!-font-weight-bold">
                You have {duplicateCount} duplicate items.
              </p>
              <Details summaryChildren="View Items">
                <ul className={styles.duplicateList}>
                  {duplicateFoldersAndFiles.folders.map((folder) => (
                    <li key={folder} className={styles.duplicateListItem}>
                      <>
                        <FolderIcon />{" "}
                        <span className="govuk-visually-hidden">Folder</span>{" "}
                        <span>{getFolderNameFromPath(folder)}</span>
                      </>
                    </li>
                  ))}
                  {duplicateFoldersAndFiles.files.map((file) => (
                    <li key={file} className={styles.duplicateListItem}>
                      <>
                        <FileIcon />{" "}
                        <span className="govuk-visually-hidden">File</span>{" "}
                        <span>{getFileNameFromPath(file)}</span>
                      </>
                    </li>
                  ))}
                </ul>
              </Details>
            </div>
          )}
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
