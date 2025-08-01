import { Button, BackLink } from "../../govuk";
import { useParams, useLocation, useNavigate } from "react-router-dom";
import styles from "./TransferErrorPage.module.scss";

const TransferErrorPage: React.FC = () => {
  const { state }: { state: { transferId: string } } = useLocation();
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
        <h1>There is a problem transferring files</h1>
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

        <p data-testid="contact-information">
          To get help, call the Service Desk{" "}
          <a href="tel:08006926996">0800 692 6996</a>. Tell them you're seeing
          error code: <b>{state.transferId}</b>.
        </p>
      </div>
    </div>
  );
};

export default TransferErrorPage;
