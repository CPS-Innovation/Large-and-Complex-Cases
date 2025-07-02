import { useCallback, useState, useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { BackLink, Button, InsetText, Tag } from "../../govuk";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import {
  getGroupedResolvePaths,
  getMappedResolvePathFiles,
  ResolvePathFileType,
} from "../../../common/utils/getGroupedResolvePaths";
import { RenameTransferFilePage } from "./RenameTransferFilePage";
import { initiateFileTransfer } from "../../../apis/gateway-api";
import styles from "./TransferResolveFilePathPage.module.scss";

const TransferResolveFilePathPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const initialValue = getMappedResolvePathFiles(
    location?.state?.validationErrors,
    location?.state?.destinationPath,
  );

  const [resolvePathFiles, setResolvePathFiles] =
    useState<ResolvePathFileType[]>(initialValue);
  const [selectedRenameFile, setSelectedRenameFile] =
    useState<ResolvePathFileType | null>(null);

  const groupedResolvedPathFiles: Record<string, ResolvePathFileType[]> =
    useMemo(() => {
      return getGroupedResolvePaths(resolvePathFiles);
    }, [resolvePathFiles]);

  const unResolvedPaths = useMemo(() => {
    return !!resolvePathFiles.find(
      (file) => file.relativeFinalPath.length + file.sourceName.length > 260,
    );
  }, [resolvePathFiles]);

  const getCharactersTag = useCallback((filePath: string) => {
    if (filePath.length > 260)
      return (
        <Tag gdsTagColour="red" className={styles.statusTag}>
          {filePath.length} characters
        </Tag>
      );
    return (
      <Tag gdsTagColour="green" className={styles.statusTag}>
        {filePath.length} characters
      </Tag>
    );
  }, []);

  const handleRenameButtonClick = (id: string) => {
    const selectedFile = resolvePathFiles.find((file) => file.id === id)!;
    setSelectedRenameFile(selectedFile);
  };

  const handleRenameCancel = () => {
    setSelectedRenameFile(null);
  };

  const handleRenameContinue = (newName: string) => {
    setResolvePathFiles((prevValues) =>
      prevValues.map((value) =>
        value.id === selectedRenameFile!.id
          ? { ...value, sourceName: newName }
          : value,
      ),
    );
    setSelectedRenameFile(null);
  };

  const handleCompleteTransferBtnClick = async () => {
    const resolvedFiles = resolvePathFiles.map((file) => ({
      id: file.id,
      sourcePath: `${file.relativeSourcePath}/${file.sourceName}`,
    }));

    const initiatePayload = {
      ...location.state.initiateTransferPayload,
      sourcePaths: [
        ...location.state.initiateTransferPayload.sourcePaths,
        ...resolvedFiles,
      ],
    };

    try {
      await initiateFileTransfer(initiatePayload);
      navigate(`/case/${initiatePayload.caseId}/case-management/`);
    } catch (error) {
      console.log("error>>>>", error);
      return;
    }
  };

  if (selectedRenameFile)
    return (
      <RenameTransferFilePage
        fileName={selectedRenameFile.sourceName}
        relativeFilePath={selectedRenameFile.relativeFinalPath}
        handleCancel={handleRenameCancel}
        handleContinue={handleRenameContinue}
      />
    );

  return (
    <div className="govuk-width-container">
      <BackLink to={"/"}>Back</BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">File paths are too long</h1>
        <InsetText>
          <p>
            You cannot complete the transfer because <b>4 file paths</b> are
            longer than the shared drive limit of 260 characters.
          </p>
          <p>
            You can fix this by choosing a different destination folder with
            smaller file path or renaming the file name.
          </p>
        </InsetText>

        <div>
          <div>
            {Object.keys(groupedResolvedPathFiles).map((key) => {
              return (
                <div key={key} className={styles.errorWrapper}>
                  <div className={styles.relativePathWrapper}>
                    <FolderIcon />
                    <span className={styles.relativePathText}>{key}</span>
                  </div>
                  <ul className={styles.errorList}>
                    {groupedResolvedPathFiles[key].map((file) => {
                      return (
                        <li
                          key={file.sourceName}
                          className={styles.errorListItem}
                        >
                          <div>
                            <FileIcon />
                            <span className={styles.fileNameText}>
                              {file.sourceName}
                            </span>
                          </div>
                          <div>
                            {getCharactersTag(
                              `${file.relativeFinalPath}/${file.sourceName}`,
                            )}
                          </div>
                          <div className={styles.renameButton}>
                            <Button
                              name="secondary"
                              className="govuk-button--secondary"
                              onClick={() => handleRenameButtonClick(file.id)}
                            >
                              Rename
                            </Button>
                          </div>
                        </li>
                      );
                    })}
                  </ul>
                </div>
              );
            })}
          </div>

          <Button
            className={styles.btnCompleteTransfer}
            disabled={unResolvedPaths}
            onClick={handleCompleteTransferBtnClick}
          >
            Complete transfer
          </Button>
        </div>
      </div>
    </div>
  );
};

export default TransferResolveFilePathPage;
