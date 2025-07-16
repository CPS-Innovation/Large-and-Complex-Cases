import { useMemo } from "react";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { Tag, Button } from "../../govuk";
import { getGroupedActvityFilePaths } from "../../../common/utils/getGroupedActivityFilePaths";
import styles from "./relativePathFiles.module.scss";
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
    () => getGroupedActvityFilePaths(successFiles, errorFiles, sourcePath),
    [successFiles, errorFiles, sourcePath],
  );

  console.log("groupedFiles>>>", groupedFiles);
  return (
    <div>
      {Object.keys(groupedFiles).map((key) => {
        return (
          <section key={key}>
            <div className={styles.relativePath}>
              <FolderIcon />
              <span>{key}</span>
            </div>
            {!!groupedFiles[`${key}`].errors.length && (
              <ul className={styles.list}>
                {groupedFiles[`${key}`].errors.map((file) => (
                  <li key={key} className={styles.listItem}>
                    <div className={styles.listContent}>
                      <Tag
                        gdsTagColour="red"
                        className={styles.statusTag}
                        data-testid="character-tag"
                      >
                        Failed
                      </Tag>{" "}
                      <FileIcon />
                      <span className={styles.fileName}>{file.fileName}</span>
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
                  <li key={key} className={styles.listItem}>
                    <div className={styles.listContent}>
                      <FileIcon />
                      <span className={styles.fileName}>{file.fileName}</span>
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
