import { useMemo, useState } from "react";
import { Button } from "../../../govuk";
import { useNavigate, useLocation } from "react-router";
import styles from "./index.module.scss";

const EgressConfirmationPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { name } = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return {
      name: params.get("name") ?? "",
    };
  }, [location.search]);

  const [disableButtons, setDisableButtons] = useState(false);

  const handleContinue = async (
    event: React.SyntheticEvent<HTMLButtonElement>,
  ) => {
    setDisableButtons(true);
    event.preventDefault();
    void navigate("/manage-set-up-shared-drive");
  };

  return (
    <div className={styles.contentWrapper}>
      <h1 className="govuk-heading-xl">Egress case created</h1>
      <p className="govuk-body">
        {name
          ? `The Egress case ${name} has been connected.`
          : "A new case has been created on Egress."}
      </p>
      <p className="govuk-body">Select Continue to manage the Shared Drive.</p>
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
  );
};

export default EgressConfirmationPage;
