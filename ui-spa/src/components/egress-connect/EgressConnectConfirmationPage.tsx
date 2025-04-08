import { useState } from "react";
import { Button, Radios, BackLink } from "../govuk";
import styles from "./EgressConnectConfirmationPage.module.scss";
type EgressConnectConfirmationPageProps = {
  selectedWorkspaceName: string;
  backLinkUrl: string;
  handleContinue: (value: boolean) => void;
  // handleBack: () => void;
};
const EgressConnectConfirmationPage: React.FC<
  EgressConnectConfirmationPageProps
> = ({ selectedWorkspaceName, backLinkUrl, handleContinue }) => {
  const [formValue, setFormValue] = useState("yes");

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    handleContinue(formValue === "yes");
  };
  return (
    <div className={styles.confirmationWrapper}>
      <BackLink to={backLinkUrl}>Back</BackLink>
      <h1>Confirm folder link</h1>
      <p>
        {`Are you sure you want to link "${selectedWorkspaceName}" Egress folder to
        the case? Once linked, this folder will be used for transferring files
        related to the case. You can update the linked folder later if needed.`}
      </p>
      <form onSubmit={handleSubmit}>
        <Radios
          aria-label="confirm folder link"
          name="confirm folder link"
          hint={{
            children: "Select one option",
          }}
          items={[
            {
              children: "Yes",
              value: "yes",
              "data-testid": "radio-egress-connect-yes",
            },
            {
              children: "No",
              value: "no",
              "data-testid": "radio-egress-connect-no",
            },
          ]}
          value={formValue}
          onChange={(value) => {
            if (value) setFormValue(value);
          }}
        ></Radios>
        <Button type="submit" onClick={() => handleSubmit}>
          {" "}
          Continue
        </Button>
      </form>
    </div>
  );
};

export default EgressConnectConfirmationPage;
