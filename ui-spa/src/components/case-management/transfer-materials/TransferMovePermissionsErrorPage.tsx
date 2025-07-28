import { Button, BackLink } from "../../govuk";
import { useParams, useNavigate } from "react-router-dom";
import styles from "./TransferMovePermissionsErrorPage.module.scss";

const TransferMovePermissionsErrorPage = () => {
  const { caseId } = useParams();
  const navigate = useNavigate();

  const handleButtonClick = () => {
    navigate(`/case/${caseId}/case-management`, {
      replace: true,
    });
  };

  return (
    <div className="govuk-width-container">
      <BackLink to={`/case/${caseId}/case-management`} replace>
        Back
      </BackLink>
      <div className={styles.contentWrapper}>
        <h1>Cannot complete move operation</h1>
        <div>
          You can:
          <ul className={styles.userActionsList}>
            <li>try again</li>
            <li>
              check the activity log to see if any files or folders have
              transferred successfully
            </li>
          </ul>
          <Button onClick={handleButtonClick} className={styles.continueBtn}>
            Continue
          </Button>
        </div>
      </div>
    </div>
  );
};

export default TransferMovePermissionsErrorPage;
