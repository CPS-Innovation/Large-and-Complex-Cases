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
      ariaDescription={duplicateCount > 0 ? "Items already exist" : "Confirm"}
    >
      <div>
        <div className={styles.modalHeader}>
          {duplicateCount > 0 ? "Items already exist" : "Confirm"}
        </div>
        <div className={styles.modalContent}>
          {duplicateCount > 0 && (
            <div
              className={styles.duplicateWarning}
              data-testid="duplicate-warning"
            >
              <p className={styles.duplicateWarningText}>
                There are some files or folders with the same name already in
                this location.
              </p>
              <Details summaryChildren="View items">
                <ul
                  className={styles.duplicateList}
                  data-testid="duplicate-folder-file-list"
                >
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
              <p>If you continue:</p>
              <ul
                className={styles.duplicateWarningList}
                data-testid="duplicate-warning-list"
              >
                <li>new items will be copied</li>
                <li>items that are duplicated will not be copied </li>
                <li>
                  details of the transfer, including items that could not be
                  copied, will be shown in the activity log.
                </li>
              </ul>
              <p>
                {transferAction.actionType === "copy"
                  ? "If you cancel, nothing will be copied"
                  : "If you cancel, nothing will be moved"}
              </p>
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
