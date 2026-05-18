import { BackLink } from "../govuk";
import { Link } from "react-router";
import { useLocation } from "react-router-dom";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import { SharedDriveConnectFailureRouteState } from "../../common/types/SharedDriveConnectFailureRouteState";
import { SharedDriveConnectRouteState } from "../../common/types/SharedDriveConnectRouteState";
import styles from "./NetAppConnectFailurePage.module.scss";

const NetAppConnectFailurePage: React.FC = () => {
  const {
    state,
  }: {
    state: SharedDriveConnectFailureRouteState;
  } = useLocation();
  const { backLinkUrl, searchQueryString, netappRootFolderPath } = state;
  const backLinkPayload: SharedDriveConnectRouteState = {
    isRouteValid: true,
    searchQueryString,
    netappRootFolderPath,
  };
  return (
    <div>
      <BackLink to={backLinkUrl} state={backLinkPayload}>
        Back
      </BackLink>
      <PageContentWrapper>
        <div className={styles.contentWrapper}>
          <h1 className="govuk-heading-xl">
            There is a problem connecting to the Shared Drive
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

export default NetAppConnectFailurePage;
