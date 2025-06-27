import { useState } from "react";
import { BackLink } from "../../govuk";
import { useLocation } from "react-router-dom";
import { Button } from "../../govuk";
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
  const [resolvePathFiles, setResolvePathFiles] =
    useState<Record<string, ResolvePathFileType[]>>(initialValue);

  return (
    <div className="govuk-width-container">
      <BackLink to={"/"}>Back</BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">File paths are too long</h1>
        <div>
          <span>
            You cannot complete the transfer because <b>4 file paths</b> are
            longer than the shared drive limit of 26 characters
          </span>
          <span>
            You can fix this by choosing a different destination folder with
            smaller file path or renaming the file name
          </span>
        </div>
        <div>
          <div className={styles.errorList}>
            {Object.keys(resolvePathFiles).map((key) => {
              return (
                <div key={key}>
                  <div>{key}</div>
                  <ul className={styles.errorList}>
                    {resolvePathFiles[key].map((file) => {
                      return (
                        <li
                          key={file.sourceName}
                          className={styles.errorListItem}
                        >
                          <span>{file.sourceName}</span>
                          <span>123</span>
                          <Button
                            name="secondary"
                            className="govuk-button--secondary"
                          >
                            abc
                          </Button>
                        </li>
                      );
                    })}
                  </ul>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
};

export default TransferResolveFilePathPage;
