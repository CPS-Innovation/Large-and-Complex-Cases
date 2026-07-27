import { useMemo } from "react";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { Tag } from "../../govuk";
import { getGroupedActivityFilePaths } from "../../../common/utils/getGroupedActivityFilePaths";
import { sortRelativePaths } from "../../../common/utils/sortRelativePaths";
import styles from "./RelativePathFiles.module.scss";
type RelativePathFilesProps = {
  successFiles: { path: string }[];
  errorFiles: { path: string }[];
  skippedFiles?: { path: string }[];
  sourcePath: string;
  name: string;
};

const RelativePathFiles: React.FC<RelativePathFilesProps> = ({
  successFiles,
  errorFiles,
  skippedFiles = [],
  sourcePath,
  name,
}) => {
  const groupedFiles = useMemo(
    () =>
      getGroupedActivityFilePaths(
        successFiles,
        errorFiles,
        sourcePath,
        skippedFiles,
      ),
    [successFiles, errorFiles, skippedFiles, sourcePath],
  );

  return (
    <div data-testid={`${name}-files`}>
      {sortRelativePaths(Object.keys(groupedFiles)).map((key) => {
        const group = groupedFiles[`${key}`];
        const hasErrors = !!group.errors.length;
        const hasSkipped = !!group.skipped.length;
        const hasSuccess = !!group.success.length;

        return (
          <section key={key}>
            <div
              className={styles.relativePathWrapper}
              data-testid={`${name}-relative-path`}
            >
              {key && <FolderIcon />}
              <span className={styles.relativePathText}>{key}</span>
            </div>

            {hasErrors && (
              <ul className={styles.list}>
                {group.errors.map((file) => (
                  <li
                    key={`${key}-err-${file.fileName}`}
                    className={styles.listItem}
                  >
                    <div className={styles.listContent}>
                      <Tag
                        gdsTagColour="red"
                        className={styles.statusTag}
                        data-testid="character-tag"
                      >
                        Failed
                      </Tag>{" "}
                      <div className={styles.fileNameWrapper}>
                        <FileIcon />
                        <span className={styles.fileNameText}>
                          {file.fileName}
                        </span>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
            {hasErrors && (hasSkipped || hasSuccess) && (
              <div className={styles.seperator} />
            )}
            {hasSkipped && (
              <ul className={styles.list}>
                {group.skipped.map((file) => (
                  <li
                    key={`${key}-skip-${file.fileName}`}
                    className={styles.listItem}
                  >
                    <div className={styles.listContent}>
                      <Tag
                        gdsTagColour="yellow"
                        className={styles.statusTag}
                        data-testid="character-tag"
                      >
                        Skipped
                      </Tag>{" "}
                      <div className={styles.fileNameWrapper}>
                        <FileIcon />
                        <span className={styles.fileNameText}>
                          {file.fileName}
                        </span>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
            {hasSkipped && hasSuccess && <div className={styles.seperator} />}
            {hasSuccess && (
              <ul className={styles.list}>
                {group.success.map((file) => (
                  <li
                    key={`${key}-ok-${file.fileName}`}
                    className={styles.listItem}
                  >
                    <div className={styles.listContent}>
                      <div className={styles.fileNameWrapper}>
                        <FileIcon />
                        <span className={styles.fileNameText}>
                          {file.fileName}
                        </span>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </section>
        );
      })}
    </div>
  );
};

export default RelativePathFiles;
