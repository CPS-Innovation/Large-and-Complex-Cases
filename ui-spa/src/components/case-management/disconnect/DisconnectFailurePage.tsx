import { useMemo } from "react";
import { Button } from "../../govuk";
import { useNavigate, useParams, useLocation } from "react-router";
import styles from "./DisconnectFailurePage.module.scss";

const DisconnectSharedDriveFailurePage = () => {
  const { caseId } = useParams() as { caseId: string };
  const navigate = useNavigate();
  const location = useLocation();
  const { queryType } = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return {
      queryType: params.get("type") ?? "",
    };
  }, [location.search]);

  const handleSubmit = (event: React.MouseEvent) => {
    event.preventDefault();

    void navigate(`/case/${caseId}/case-management`);
  };

  return (
    <div className={styles.contentWrapper}>
      {queryType === "shared-drive" ? (
        <h1>Could not disconnect the Shared Drive folder</h1>
      ) : (
        <h1>Could not disconnect Egress</h1>
      )}
      <p>Try again.</p>
      <p>If the problem continues, contact the product team for support.</p>

      <div className={styles.buttonWrapper}>
        <Button onClick={handleSubmit}>continue</Button>
      </div>
    </div>
  );
};

export default DisconnectSharedDriveFailurePage;
