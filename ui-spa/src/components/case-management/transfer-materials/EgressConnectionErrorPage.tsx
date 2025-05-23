import { Button, LinkButton, BackLink } from "../../govuk";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import styles from "./egressConnectionErrorPage.module.scss";

const EgressConnectionErrorPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId } = useParams();
  const searchParams = new URLSearchParams(location.search);
  const operationName = searchParams.get("operation-name");
  const handleCancel = () => {
    navigate("/");
  };
  const handleReconnect = () => {
    console.log(
      " location.state.netappFolderPath>>",
      location.state.netappFolderPath,
    );
    navigate(`/case/${caseId}/egress-connect?workspace-name=${operationName}`, {
      state: {
        netappFolderPath: location.state.netappFolderPath,
      },
    });
  };
  return (
    <div className="govuk-width-container">
      <BackLink to={"/"}>Back</BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">
          There was a problem connecting to egress
        </h1>
        <p>
          The connection to egress folder for <b>{operationName}</b> case has
          stopped working
        </p>
        <div className={styles.btnWrapper}>
          <Button onClick={() => handleReconnect()}>Reconnect</Button>
          <LinkButton onClick={() => handleCancel()}>Cancel</LinkButton>
        </div>
      </div>
    </div>
  );
};

export default EgressConnectionErrorPage;
