import { useCallback, useState, useMemo } from "react";
import { useLocation } from "react-router-dom";
import { BackLink, Button, InsetText, Tag } from "../../govuk";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import {
  getGroupedResolvePaths,
  ResolvePathFileType,
} from "../../../common/utils/getGroupedResolvePaths";
import styles from "./TransferResolveFilePathPage.module.scss";

const TransferResolveFilePathPage = () => {
  const location = useLocation();
  const initialValue = getGroupedResolvePaths(
    location?.state?.validationErrors,
  );
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [resolvePathFiles, setResolvePathFiles] =
    useState<Record<string, ResolvePathFileType[]>>(initialValue);

  const unResolvedPaths = useMemo(() => {
    const filePaths = Object.keys(resolvePathFiles).reduce((acc, curr) => {
      const resolvedFilePaths = resolvePathFiles[`${curr}`].map(
        (file) =>
          `${location?.state?.destinationPath}/${file.relativePath}/${file.sourceName}`,
      );
      acc = [...acc, ...resolvedFilePaths];
      return acc;
    }, [] as string[]);
    return !!filePaths.find((path) => path.length > 260);
  }, [resolvePathFiles, location?.state?.destinationPath]);

  const getCharactersTag = useCallback(
    (sourcePath: string) => {
      const totalCharacters =
        `${location?.state?.destinationPath}/${sourcePath}`.length;
      if (totalCharacters > 260)
        return (
          <Tag gdsTagColour="red" className={styles.statusTag}>
            {totalCharacters} characters
          </Tag>
        );
      return (
        <Tag gdsTagColour="green" className={styles.statusTag}>
          {totalCharacters} characters
        </Tag>
      );
    },
    [location?.state?.destinationPath],
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
            {Object.keys(resolvePathFiles).map((key) => {
              return (
                <div key={key} className={styles.errorWrapper}>
                  <div className={styles.relativePathWrapper}>
                    <FolderIcon />
                    <span className={styles.relativePathText}>{key}</span>
                  </div>
                  <ul className={styles.errorList}>
                    {resolvePathFiles[key].map((file) => {
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
                              `${file.relativePath}/${file.sourceName}`,
                            )}
                          </div>
                          <div className={styles.renameButton}>
                            <Button
                              name="secondary"
                              className="govuk-button--secondary"
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
          >
            Complete transfer
          </Button>
        </div>
      </div>
    </div>
  );
};

export default TransferResolveFilePathPage;
