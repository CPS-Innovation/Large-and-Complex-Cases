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
  sourcePath: string;
};

const RelativePathFiles: React.FC<RelativePathFilesProps> = ({
  successFiles,
  errorFiles,
  sourcePath,
}) => {
  const groupedFiles = useMemo(
    () => getGroupedActivityFilePaths(successFiles, errorFiles, sourcePath),
    [successFiles, errorFiles, sourcePath],
  );

  return (
    <div data-testid="activity-files">
      {sortRelativePaths(Object.keys(groupedFiles)).map((key) => {
        return (
          <section key={key}>
            <div
              className={styles.relativePathWrapper}
              data-testid="activity-relative-path"
            >
              {key && <FolderIcon />}
              <span className={styles.relativePathText}>{key}</span>
            </div>

            {!!groupedFiles[`${key}`].errors.length && (
              <ul className={styles.list}>
                {groupedFiles[`${key}`].errors.map((file) => (
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
            {!!groupedFiles[`${key}`].errors.length &&
              !!groupedFiles[`${key}`].success.length && (
                <div className={styles.seperator} />
              )}
            {!!groupedFiles[`${key}`].success.length && (
              <ul className={styles.list}>
                {groupedFiles[`${key}`].success.map((file) => (
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
