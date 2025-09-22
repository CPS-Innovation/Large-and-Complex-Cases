import { useState } from "react";
import { Button, Radios, BackLink } from "../govuk";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import styles from "./NetAppConnectConfirmationPage.module.scss";
type NetAppConnectConfirmationPageProps = {
  selectedFolderName: string;
  backLinkUrl: string;
  handleContinue: (value: boolean) => void;
};
const NetAppConnectConfirmationPage: React.FC<
  NetAppConnectConfirmationPageProps
> = ({ selectedFolderName, backLinkUrl, handleContinue }) => {
  const [formValue, setFormValue] = useState("yes");

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    handleContinue(formValue === "yes");
  };
  return (
    <div className={styles.confirmationWrapper}>
      <BackLink to={backLinkUrl}>Back</BackLink>
      <PageContentWrapper>
        <form onSubmit={handleSubmit}>
          <Radios
            className="govuk-radios--inline"
            fieldset={{
              legend: {
                children: (
                  <>
                    <h1 className="govuk-fieldset__legend--xl">
                      Are you sure?
                    </h1>{" "}
                    <span>
                      {`Confirm you want to link "${selectedFolderName}" network shared drive folder to
              the case?`}
                    </span>
                  </>
                ),
              },
            }}
            name="Are you sure?"
            hint={{
              children: "You can change the linked folder later if needed.",
            }}
            items={[
              {
                children: "Yes",
                value: "yes",
                "data-testid": "radio-netapp-connect-yes",
              },
              {
                children: "No",
                value: "no",
                "data-testid": "radio-netapp-connect-no",
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
      </PageContentWrapper>
    </div>
  );
};

export default NetAppConnectConfirmationPage;
