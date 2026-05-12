import { Panel } from "../../govuk";
import { useLocation, Link } from "react-router-dom";
import styles from "./DisconnectSharedDriveSuccessPage.module.scss";
const DisconnectSharedDriveSuccessPage = () => {
  const {
    state: { urn },
  }: {
    state: {
      urn: string;
    };
  } = useLocation();
  return (
    <div className={styles.contentWrapper}>
      <Panel titleChildren="Shared Drive disconnected"></Panel>
      <p>You&apos;ve disconnected the Shared Drive folder.</p>
      <p>You can connect a different folder if you need to.</p>
      <Link
        to={`/search-results?urn=${urn}`}
        className="govuk-link--no-visited-state"
      >
        Connect a folder
      </Link>
    </div>
  );
};
export default DisconnectSharedDriveSuccessPage;
