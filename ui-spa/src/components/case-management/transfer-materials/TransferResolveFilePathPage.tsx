import { useCallback, useState, useMemo, useEffect, useContext } from "react";
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
  ResolvePathFileType,
} from "../../../common/utils/getGroupedResolvePaths";
import { getMappedResolvePathFiles } from "../../../common/utils/getMappedResolvePathFiles";
import { RenameTransferFilePage } from "./RenameTransferFilePage";
import { initiateFileTransfer } from "../../../apis/gateway-api";
import { EgressTransferPayloadSourcePath } from "../../../schemas/requests/initiateFileTransferPayload";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import { MainStateContext } from "../../../providers/MainStateProvider";
import styles from "./TransferResolveFilePathPage.module.scss";

const MAX_FILE_PATH_CHARACTERS = 260;

const TransferResolveFilePathPage = () => {
  const { state } = useContext(MainStateContext);
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId } = useParams();
  const [ariaLiveText, setAriaLiveText] = useState("");

  const [resolvePathFiles, setResolvePathFiles] = useState<
    ResolvePathFileType[]
  >([]);
  const [selectedRenameFile, setSelectedRenameFile] =
    useState<ResolvePathFileType | null>(null);

  const [disableBtns, setDisableBtns] = useState(false);

  const groupedResolvedPathFiles: Record<string, ResolvePathFileType[]> =
    useMemo(() => {
      return getGroupedResolvePaths(resolvePathFiles);
    }, [resolvePathFiles]);

  const getFullTransferPath = useCallback(
    (file: ResolvePathFileType) =>
      `${file.relativeFinalPath}${file.sourceName}`,
    [],
  );

  // Count unresolved files from the backend-flagged list using the same full
  // path string shown in tags/Rename (from destinationFullPath).
  const largePathFilesCount = useMemo(() => {
    return resolvePathFiles.filter(
      (file) => getFullTransferPath(file).length > MAX_FILE_PATH_CHARACTERS,
    ).length;
  }, [resolvePathFiles, getFullTransferPath]);

  const hasUnresolvedPathErrors = largePathFilesCount > 0;

  useEffect(() => {
    setAriaLiveText(
      "indexing transfer error, file paths are too long to transfer",
    );
  }, []);

  useEffect(() => {
    const { validationErrors, destinationPath } =
      state.appData.transferResolveFilePathPage;
    if (validationErrors && destinationPath) {
      const initialValue = getMappedResolvePathFiles(validationErrors);
      setResolvePathFiles(initialValue);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getCharactersTag = useCallback((filePath: string) => {
    if (filePath.length > MAX_FILE_PATH_CHARACTERS)
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
    const selectedFile = resolvePathFiles.find((file) => file.id === id);
    if (!selectedFile) {
      return;
    }
    setSelectedRenameFile(selectedFile);
    navigate(`/case/${caseId}/case-management/transfer-rename-file`, {
      replace: true,
    });
  };

  const handleRenameCancel = () => {
    navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`, {
      replace: true,
    });
    setSelectedRenameFile(null);
  };

  const handleRenameContinue = (newName: string) => {
    if (!selectedRenameFile) {
      return;
    }
    setResolvePathFiles((prevValues) =>
      prevValues.map((value) =>
        value.id === selectedRenameFile.id
          ? { ...value, sourceName: newName }
          : value,
      ),
    );
    setSelectedRenameFile(null);
    navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`, {
      replace: true,
    });
  };

  const handleStartTransferBtnClick = async () => {
    const { initiateTransferPayload } =
      state.appData.transferResolveFilePathPage;
    if (!initiateTransferPayload) {
      return;
    }
    setDisableBtns(true);
    const resolvedFiles: EgressTransferPayloadSourcePath[] =
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
      ...initiateTransferPayload,
      sourcePaths: [
        ...(initiateTransferPayload?.sourcePaths ?? []),
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
      console.log("error", error);
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
    <div>
      <BackLink to={`/case/${caseId}/case-management`} replace>
        Back
      </BackLink>
      {hasUnresolvedPathErrors && (
        <span role="alert" aria-live="polite" className="govuk-visually-hidden">
          {ariaLiveText}
        </span>
      )}
      <PageContentWrapper>
        {!hasUnresolvedPathErrors && resolvePathFiles.length > 0 && (
          <div className={styles.successBanner}>
            <NotificationBanner
              type="success"
              data-testid="resolve-path-success-notification-banner"
            >
              <b className={styles.successMessage}>
                Your {resolvePathFiles.length === 1 ? "file" : "files"} can now
                be transferred.
              </b>
            </NotificationBanner>
          </div>
        )}
        <div className={styles.contentWrapper}>
          {hasUnresolvedPathErrors && (
            <>
              <h1 className="govuk-heading-xl">
                File paths are too long to transfer
              </h1>
              <InsetText data-testid="resolve-file-path-inset-text">
                {largePathFilesCount === 1 ? (
                  <p>
                    <b>1 file</b> exceeds the 260 character limit.
                  </p>
                ) : (
                  <p>
                    <b>{largePathFilesCount} files</b> exceed the 260 character
                    limit.
                  </p>
                )}

                <div>
                  <p>The full file path includes:</p>
                  <ul>
                    <li>the destination folder</li>
                    <li> the existing folder structure</li>
                    <li> the file name</li>
                  </ul>
                </div>

                <div>
                  <p>To continue, you can:</p>
                  <ul>
                    <li>shorten the file names</li>
                    <li>move the files to a folder with a shorter path</li>
                  </ul>
                </div>
              </InsetText>
            </>
          )}

          <div>
            <div>
              {Object.keys(groupedResolvedPathFiles).map((key) => {
                return (
                  <section key={key} className={styles.errorWrapper}>
                    <div className={styles.relativePathWrapper}>
                      <FolderIcon />
                      <span
                        className={styles.relativePathText}
                        data-testid="relative-path"
                      >
                        {key}
                      </span>
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
                              {getCharactersTag(getFullTransferPath(file))}
                            </div>
                            <div className={styles.renameButton}>
                              <Button
                                name="secondary"
                                className="govuk-button--secondary"
                                disabled={disableBtns}
                                onClick={() => handleRenameButtonClick(file.id)}
                                aria-label={`rename file ${file.sourceName}`}
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
            {resolvePathFiles.length > 0 && (
              <div className={styles.btnWrapper}>
                <Button
                  disabled={disableBtns || hasUnresolvedPathErrors}
                  onClick={handleStartTransferBtnClick}
                >
                  Start transfer
                </Button>
                <LinkButton onClick={handleCancel} disabled={disableBtns}>
                  Cancel
                </LinkButton>
              </div>
            )}
          </div>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default TransferResolveFilePathPage;
