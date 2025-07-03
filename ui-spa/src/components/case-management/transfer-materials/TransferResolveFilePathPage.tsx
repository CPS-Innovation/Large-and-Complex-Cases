import { useCallback, useState, useMemo, useEffect } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
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
import { EgressTranferPayloadSourcePath } from "../../../common/types/InitiateFileTransferPayload";
import { TransferResolvePageLocationState } from "../../../common/types/TransferResolvePageLocationState";
import styles from "./TransferResolveFilePathPage.module.scss";

const TransferResolveFilePathPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId } = useParams();

  const [locationState, setLocationState] =
    useState<TransferResolvePageLocationState>();

  const [resolvePathFiles, setResolvePathFiles] = useState<
    ResolvePathFileType[]
  >([]);
  const [selectedRenameFile, setSelectedRenameFile] =
    useState<ResolvePathFileType | null>(null);

  const [disableTransferBtn, setDisableTransferBtn] = useState(true);

  const groupedResolvedPathFiles: Record<string, ResolvePathFileType[]> =
    useMemo(() => {
      return getGroupedResolvePaths(
        resolvePathFiles,
        locationState ? locationState?.baseFolderName : "",
      );
    }, [resolvePathFiles, locationState]);

  useEffect(() => {
    const largePathFiles = resolvePathFiles.find(
      (file) => file.relativeFinalPath.length + file.sourceName.length > 260,
    );

    setDisableTransferBtn(!!largePathFiles);
  }, [resolvePathFiles]);

  useEffect(() => {
    const initialValue = getMappedResolvePathFiles(
      location?.state?.validationErrors,
      location?.state?.destinationPath,
    );
    setResolvePathFiles(initialValue);
    if (!locationState) {
      setLocationState(location.state);
    }
  }, []);

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
    navigate(`/case/${caseId}/case-management/transfer-rename-file`);
  };

  const handleRenameCancel = () => {
    navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`);
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
    navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`);
  };

  const handleCompleteTransferBtnClick = async () => {
    if (!locationState?.initiateTransferPayload) {
      return;
    }
    setDisableTransferBtn(true);
    const resolvedFiles: EgressTranferPayloadSourcePath[] =
      resolvePathFiles.map((file) => ({
        fileId: file.id,
        path: file.relativeSourcePath
          ? `${file.relativeSourcePath}/${file.sourceName}`
          : file.sourceName,
        modifiedPath: file.relativeSourcePath
          ? `${file.relativeSourcePath}/${file.sourceName}`
          : file.sourceName,
      }));

    const initiatePayload = {
      ...locationState?.initiateTransferPayload,
      sourcePaths: [
        ...(locationState?.initiateTransferPayload?.sourcePaths ?? []),
        ...resolvedFiles,
      ],
    };

    try {
      const response = await initiateFileTransfer(initiatePayload);
      navigate(`/case/${caseId}/case-management/`, {
        state: {
          transferId: response.id,
        },
      });
    } catch (error) {
      console.log("error>>>>", error);
      return;
    }
  };

  if (location.pathname.endsWith("/transfer-rename-file") && selectedRenameFile)
    return (
      <RenameTransferFilePage
        backLinkUrl={`/case/${caseId}/case-management/transfer-resolve-file-path`}
        fileName={selectedRenameFile.sourceName}
        relativeFilePath={selectedRenameFile.relativeFinalPath}
        handleCancel={handleRenameCancel}
        handleContinue={handleRenameContinue}
      />
    );

  return (
    <div className="govuk-width-container">
      <BackLink to={`/case/${caseId}/case-management/`}>Back</BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">File paths are too long</h1>
        <InsetText>
          <p>
            You cannot complete the transfer because{" "}
            <b>{resolvePathFiles.length} file paths</b> are longer than the
            shared drive limit of 260 characters.
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
            disabled={disableTransferBtn}
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
