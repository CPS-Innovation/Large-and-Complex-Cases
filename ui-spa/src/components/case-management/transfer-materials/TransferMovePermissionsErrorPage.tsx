import { useEffect } from "react";
import { Button, BackLink } from "../../govuk";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import styles from "./TransferMovePermissionsErrorPage.module.scss";

const TransferMovePermissionsErrorPage = () => {
  const { state }: { state: { isRouteValid: boolean } } = useLocation();
  const { isRouteValid = false } = state || {};
  const { caseId } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isRouteValid) {
      navigate(`/`);
    }
  }, []);

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
              To get help, call the Service Desk{" "}
              <a href="tel:08006926996">0800 692 6996</a>.
            </p>
          </div>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default TransferMovePermissionsErrorPage;
