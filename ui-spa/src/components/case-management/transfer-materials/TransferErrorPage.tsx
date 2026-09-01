import { Button, BackLink, Details } from "../../govuk";
import { useContext } from "react";
import { useParams, useNavigate } from "react-router";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import FileIcon from "../../../components/svgs/file.svg?react";
import { MainStateContext } from "../../../providers/MainStateProvider";
import styles from "./TransferErrorPage.module.scss";

const TransferErrorPage: React.FC = () => {
  const { state } = useContext(MainStateContext);
  const { transferId = "", failedItems = [] } =
    state.appData?.transferErrorsPage || {};
  const { caseId } = useParams();
  const navigate = useNavigate();

  const handleButtonClick = () => {
    navigate(`/case/${caseId}/case-management`, {
      replace: true,
    });
  };

  const fileExistsFailedItems = failedItems.filter(
    (item) => item.errorCode === "FileExists",
  );

  const sourceFileNotFoundItems = failedItems.filter(
    (item) => item.errorCode === "SourceFileNotFound",
  );

  const otherFailedItems = failedItems.filter(
    (item) =>
      item.errorCode !== "FileExists" &&
      item.errorCode !== "SourceFileNotFound",
  );

  return (
    <div>
      <BackLink to={`/case/${caseId}/case-management`} replace>
        Back
      </BackLink>
      <PageContentWrapper>
        <div className={styles.contentWrapper}>
          <h1>There is a problem transferring files</h1>
          {fileExistsFailedItems.length > 0 && (
            <div data-testid="file-exists-error-wrapper">
              <h2>Some files already exist in the destination folder</h2>
              <p>You cannot transfer files that already exist.</p>
              <Details summaryChildren="View files">
                <ul
                  className={styles.failedFilesList}
                  data-testid="already-exist-files-list"
                >
                  {fileExistsFailedItems.map((item) => (
                    <li key={item.sourcePath} className={styles.failedFileItem}>
                      <>
                        <FileIcon />
                        <span className="govuk-visually-hidden">File</span>
                        <span>{item.sourcePath}</span>
                      </>
                    </li>
                  ))}
                </ul>
              </Details>
            </div>
          )}
          {sourceFileNotFoundItems.length > 0 && (
            <div data-testid="source-not-found-error-wrapper">
              <h2>Some source files could not be found</h2>
              <p>
                The files may have been moved, deleted, or are not yet available
                to transfer.
              </p>
              <Details summaryChildren="View files">
                <ul
                  className={styles.failedFilesList}
                  data-testid="source-not-found-files-list"
                >
                  {sourceFileNotFoundItems.map((item) => (
                    <li key={item.sourcePath} className={styles.failedFileItem}>
                      <>
                        <FileIcon />
                        <span className="govuk-visually-hidden">File</span>
                        <span>{item.sourcePath}</span>
                      </>
                    </li>
                  ))}
                </ul>
              </Details>
            </div>
          )}
          {otherFailedItems.length > 0 && (
            <div data-testid="other-failed-error-wrapper">
              <h2>Some files could not be transferred</h2>
              <p>Check the activity log for more details.</p>
              <Details summaryChildren="View files">
                <ul
                  className={styles.failedFilesList}
                  data-testid="other-failed-files-list"
                >
                  {otherFailedItems.map((item) => (
                    <li key={item.sourcePath} className={styles.failedFileItem}>
                      <>
                        <FileIcon />
                        <span className="govuk-visually-hidden">File</span>
                        <span>{item.sourcePath}</span>
                      </>
                    </li>
                  ))}
                </ul>
              </Details>
            </div>
          )}
          <div data-testid="user-actions-wrapper">
            <h2>What you can do</h2>
            <ul className={styles.userActionsList}>
              {fileExistsFailedItems.length > 0 && (
                <li>remove or rename any duplicate files, then try again</li>
              )}
              {sourceFileNotFoundItems.length > 0 && (
                <li>
                  check that the source files exist and are accessible, then try
                  again
                </li>
              )}
              {!fileExistsFailedItems.length &&
                !sourceFileNotFoundItems.length && <li> try again</li>}

              <li>
                check the activity log to see if any files transferred
                successfully
              </li>
              <li>
                contact the product team for help and include the error message{" "}
                <b>failed transfer - {transferId}</b>
              </li>
            </ul>
            <Button onClick={handleButtonClick} className={styles.continueBtn}>
              Continue
            </Button>
          </div>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default TransferErrorPage;
