import { useMemo, useState } from "react";
import { Button } from "../../govuk";
import { useLocation } from "react-router";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import styles from "./EgressConnectedConfirmationPage.module.scss";

const EgressConnectedConfirmationPage = () => {
  const location = useLocation();
  const egressWorkspaceName = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return params.get("egress-workspace-name") ?? "";
  }, [location.search]);

  const [disableButtons, setDisableButtons] = useState(false);

  const handleContinue = async (
    event: React.SyntheticEvent<HTMLButtonElement>,
  ) => {
    setDisableButtons(true);
    event.preventDefault();
    // Navigate the user to the Shared Drive set up page
  };

  return (
    <PageContentWrapper>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">Egress case created</h1>
        <p className="govuk-body">
          {egressWorkspaceName
            ? `The Egress case ${egressWorkspaceName} has been connected.`
            : "A new case has been created on Egress."}
        </p>
        <p className="govuk-body">
          Select Continue to manage the Shared Drive.
        </p>
        <div>
          <Button
            type="submit"
            disabled={disableButtons}
            onClick={handleContinue}
          >
            Continue
          </Button>
        </div>
      </div>
    </PageContentWrapper>
  );
};

export default EgressConnectedConfirmationPage;
