import { Button } from "../../govuk";
import { useLocation, Link, useNavigate } from "react-router-dom";
import { disconnectNetAppFolder } from "../../../apis/gateway-api";
import styles from "./DisconnectSharedDriveConfirmationPage.module.scss";

const DisconnectSharedDriveFailurePage = () => {
  const {
    state: { caseId, urn },
  }: {
    state: {
      caseId: number;
    };
  } = useLocation();
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const response = await disconnectNetAppFolder(caseId);
    if (!response.ok) return;
    navigate(
      `/case/${caseId}/case-management/disconnect-shared-drive-success`,
      { state: { urn } },
    );
  };

  return (
    <div className={styles.contentWrapper}>
      <h1>There is a problem</h1>
      <p>The Shared Drive folder could not be disconnected. Try again.</p>
      <p>If the problem continues, contact the product team for support.</p>

      <div className={styles.buttonWrapper}>
        <Button type="submit" onClick={() => handleSubmit}>
          Continue
        </Button>
        <Link to={`/case/${caseId}/case-management`}>cancel</Link>
      </div>
    </div>
  );
};

export default DisconnectSharedDriveFailurePage;
