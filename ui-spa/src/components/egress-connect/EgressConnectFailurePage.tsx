import { BackLink } from "../govuk";
import { Link } from "react-router";
import styles from "./EgressConnectFailurePage.module.scss";
type EgressConnectFailurePageProps = {
  backLinkUrl: string;
};
const EgressConnectFailurePage: React.FC<EgressConnectFailurePageProps> = ({
  backLinkUrl,
}) => {
  return (
    <div>
      <BackLink href={backLinkUrl}>Back</BackLink>

      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">
          Sorry, there was a problem connecting to egress
        </h1>
        <p>You can:</p>
        <ul className="govuk-list govuk-list--bullet">
          <li>check for spelling mistakes in the operation name</li>
          <li>
            check the Case Management System to make sure the case exists and
            that you have access
          </li>
          <li>contact the product team if you need further help</li>
        </ul>

        <Link to="/">Search for another case.</Link>
      </div>
    </div>
  );
};

export default EgressConnectFailurePage;
