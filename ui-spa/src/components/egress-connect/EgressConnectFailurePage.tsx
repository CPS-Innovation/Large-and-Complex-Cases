import { BackLink } from "../govuk";
import { Link } from "react-router";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import styles from "./EgressConnectFailurePage.module.scss";
type EgressConnectFailurePageProps = {
  backLinkUrl: string;
};
const EgressConnectFailurePage: React.FC<EgressConnectFailurePageProps> = ({
  backLinkUrl,
}) => {
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
            <li>
              check the case exists and you have access on the Case Management
              System
            </li>
            <li>contact the product team if you need help</li>
          </ul>

          <Link className={styles.link} to="/">
            Search for another case.
          </Link>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default EgressConnectFailurePage;
