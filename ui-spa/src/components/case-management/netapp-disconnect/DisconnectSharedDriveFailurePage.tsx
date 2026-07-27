import { Button } from "../../govuk";
import { useNavigate, useParams } from "react-router-dom";
import styles from "./DisconnectSharedDriveFailurePage.module.scss";

const DisconnectSharedDriveFailurePage = () => {
  const { caseId } = useParams() as { caseId: string };
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    navigate(`/case/${caseId}/case-management`);
  };

  return (
    <div className={styles.contentWrapper}>
      <h1>Could not disconnect the Shared Drive folder</h1>
      <p>Try again.</p>
      <p>If the problem continues, contact the product team for support.</p>

      <div className={styles.buttonWrapper}>
        <Button onClick={handleSubmit}>continue</Button>
      </div>
    </div>
  );
};

export default DisconnectSharedDriveFailurePage;
