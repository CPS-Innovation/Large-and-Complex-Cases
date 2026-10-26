import { useContext } from "react";
import { BackLink, Button } from "../../govuk";
import { useNavigate } from "react-router";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import { MainStateContext } from "../../../providers/MainStateProvider";
import styles from "./EgressConnectFailurePageV2.module.scss";

const EgressConnectFailurePageV2: React.FC = () => {
  const { state } = useContext(MainStateContext);
  const { backLinkUrl } =
    state.appData.caseRegistrationConnectEgressFailurePage;
  const navigate = useNavigate();

  const handleTryAgain = () => {
    void navigate(backLinkUrl);
  };

  return (
    <div>
      <BackLink to={backLinkUrl}>Back</BackLink>

      <PageContentWrapper>
        <div className={styles.contentWrapper}>
          <h1 className="govuk-heading-xl">
            There is a problem connecting to Egress
          </h1>
          <p>You can:</p>
          <ul className="govuk-list govuk-list--bullet">
            <li>return to the previous screen and try to connect again</li>
            <li>contact the product team if you need help</li>
          </ul>

          <Button onClick={handleTryAgain}> Try again</Button>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default EgressConnectFailurePageV2;
