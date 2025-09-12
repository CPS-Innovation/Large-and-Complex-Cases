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
  ResolvePathFileType,
} from "../../../common/utils/getGroupedResolvePaths";
import { getMappedResolvePathFiles } from "../../../common/utils/getMappedResolvePathFiles";
import { RenameTransferFilePage } from "./RenameTransferFilePage";
import { initiateFileTransfer } from "../../../apis/gateway-api";
import { EgressTranferPayloadSourcePath } from "../../../common/types/InitiateFileTransferPayload";
import { TransferResolvePageLocationState } from "../../../common/types/TransferResolvePageLocationState";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import styles from "./TransferResolveFilePathPage.module.scss";

const MAX_FILE_PATH_CHARACTERS = 260;

const TransferResolveFilePathPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId } = useParams();
  const [ariaLiveText, setAriaLiveText] = useState("");
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

  const largePathFiles = useMemo(() => {
    if (!resolvePathFiles.length) {
      return true;
    }
    const longPathFile = resolvePathFiles.find(
      (file) =>
        `${file.relativeFinalPath}/${file.sourceName}`.length >
        MAX_FILE_PATH_CHARACTERS,
    );
    return !!longPathFile;
  }, [resolvePathFiles]);

  useEffect(() => {
    setAriaLiveText("indexing transfer error, file structure is too long");
  }, []);

  useEffect(() => {
    const initialValue = getMappedResolvePathFiles(
      location?.state?.validationErrors,
      location?.state?.destinationPath,
    );
    setResolvePathFiles(initialValue);
    if (!locationState) {
      setLocationState(location.state);
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
    <div className="govuk-width-container">
      <BackLink to={`/case/${caseId}/case-management`} replace>
        Back
      </BackLink>
      {largePathFiles && (
        <span role="alert" aria-live="polite" className="govuk-visually-hidden">
          {ariaLiveText}
        </span>
      )}
      <PageContentWrapper>
        {!largePathFiles && (
          <div className={styles.successBanner}>
            <NotificationBanner
              type="success"
              data-testid="resolve-path-success-notification-banner"
            >
              Your {resolvePathFiles.length === 1 ? "file" : "files"} can now be
              transferred.
            </NotificationBanner>
          </div>
        )}
        <div className={styles.contentWrapper}>
          <h1 className="govuk-heading-xl">File structure is too long</h1>
          <InsetText data-testId="resolve-file-path-inset-text">
            <p>
              There {resolvePathFiles.length === 1 ? "is" : "are"}{" "}
              <b>
                {resolvePathFiles.length}{" "}
                {resolvePathFiles.length === 1 ? "file" : "files"}{" "}
              </b>
              with {resolvePathFiles.length === 1 ? "a name" : "names"} longer
              than {MAX_FILE_PATH_CHARACTERS} characters.
            </p>
            <p>
              You need to rename the{" "}
              {resolvePathFiles.length === 1 ? "file" : "files"} or change the
              folder structure.
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
            <div className={styles.btnWrapper}>
              <Button
                className={styles.btnStartTransfer}
                disabled={disableBtns || largePathFiles}
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
      </PageContentWrapper>
    </div>
  );
};

export default TransferResolveFilePathPage;
