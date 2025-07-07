import { useCallback, useState, useMemo, useEffect } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import {
  BackLink,
  Button,
  InsetText,
  Tag,
  NotificationBanner,
  LinkButton,
} from "../../govuk";
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

  const [disableBtns, setDisableBtns] = useState(false);

  const groupedResolvedPathFiles: Record<string, ResolvePathFileType[]> =
    useMemo(() => {
      return getGroupedResolvePaths(
        resolvePathFiles,
        locationState ? locationState?.baseFolderName : "",
      );
    }, [resolvePathFiles, locationState]);

  const largePathFiles = useMemo(
    () =>
      resolvePathFiles.find(
        (file) => `${file.relativeFinalPath}/${file.sourceName}`.length > 260,
      ),
    [resolvePathFiles],
  );

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
    navigate(`/case/${caseId}/case-management/transfer-rename-file`, {
      replace: true,
      state: { isRouteValid: true },
    });
  };

  const handleRenameCancel = () => {
    navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`, {
      replace: true,
      state: { isRouteValid: true },
    });
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
    navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`, {
      replace: true,
      state: { isRouteValid: true },
    });
  };

  const handleStartTransferBtnClick = async () => {
    if (!locationState?.initiateTransferPayload) {
      return;
    }
    setDisableBtns(true);
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
      navigate(`/case/${caseId}/case-management`, {
        replace: true,
        state: {
          transferId: response.id,
        },
      });
    } catch (error) {
      console.log("error>>>>", error);
      return;
    }
  };

  const handleCancel = async () => {
    navigate(`/case/${caseId}/case-management`, { replace: true });
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
      <BackLink to={`/case/${caseId}/case-management`} replace>
        Back
      </BackLink>
      {!largePathFiles && (
        <div className={styles.successBanner}>
          <NotificationBanner
            type="success"
            data-testid="resolve-path-success-notification-banner"
          >
            All file are now under the 260 character limit
          </NotificationBanner>
        </div>
      )}
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">File paths are too long</h1>
        <InsetText data-testId="resolve-file-path-inset-text">
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
                <section key={key} className={styles.errorWrapper}>
                  <div className={styles.relativePathWrapper}>
                    <FolderIcon />
                    <h2 className={styles.relativePathText}>{key}</h2>
                  </div>
                  <ul className={styles.errorList}>
                    {groupedResolvedPathFiles[key].map((file) => {
                      return (
                        <li
                          key={file.sourceName}
                          className={styles.errorListItem}
                        >
                          <div data-testid="file-name-wrapper">
                            <FileIcon />
                            <span className={styles.fileNameText}>
                              {file.sourceName}
                            </span>
                          </div>
                          <div data-testid="character-tag">
                            {getCharactersTag(
                              `${file.relativeFinalPath}/${file.sourceName}`,
                            )}
                          </div>
                          <div className={styles.renameButton}>
                            <Button
                              name="secondary"
                              className="govuk-button--secondary"
                              disabled={disableBtns}
                              onClick={() => handleRenameButtonClick(file.id)}
                            >
                              Rename
                            </Button>
                          </div>
                        </li>
                      );
                    })}
                  </ul>
                </section>
              );
            })}
          </div>
          <div className={styles.btnWrapper}>
            <Button
              className={styles.btnStartTransfer}
              disabled={disableBtns || !!largePathFiles}
              onClick={handleStartTransferBtnClick}
            >
              Start transfer
            </Button>
            <LinkButton onClick={handleCancel} disabled={disableBtns}>
              Cancel
            </LinkButton>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TransferResolveFilePathPage;
