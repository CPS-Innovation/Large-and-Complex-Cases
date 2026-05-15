import { Button, BackLink } from "../../govuk";
import { useParams, useNavigate } from "react-router-dom";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
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
    <div>
      <BackLink to={`/case/${caseId}/case-management`} replace>
        Back
      </BackLink>
      <PageContentWrapper>
        <div className={styles.contentWrapper}>
          <h1>
            You do not have permission to transfer these files from Egress
          </h1>
          <div>
            <p>
              If you think you should have access, contact the Egress
              administrator for the case.
            </p>
            <Button
              onClick={handleButtonClick}
              className={styles.returnToCaseBtn}
            >
              Return to the case
            </Button>

            <p data-testid="contact-information">
              To get help, contact the product team.
            </p>
          </div>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default TransferMovePermissionsErrorPage;
