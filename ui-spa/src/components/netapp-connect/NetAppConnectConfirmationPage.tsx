import { useState } from "react";
import { Button, Radios, BackLink } from "../govuk";
import { useLocation, useNavigate } from "react-router-dom";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import { connectNetAppFolder } from "../../apis/gateway-api";
import { getFolderNameFromPath } from "../../common/utils/getFolderNameFromPath";
import styles from "./NetAppConnectConfirmationPage.module.scss";

const NetAppConnectConfirmationPage: React.FC = () => {
  const {
    state,
  }: {
    state?: {
      caseId: string;
      operationName: string;
      selectedWorkspace: {
        folderPath: string;
      };
      backLinkUrl: string;
    };
  } = useLocation();
  const [formValue, setFormValue] = useState("yes");
  const { caseId, operationName, backLinkUrl, selectedWorkspace } = state || {};
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (formValue === "no" && state?.backLinkUrl) {
      navigate(state.backLinkUrl, {
        state: {
          isRouteValid: true,
        },
      });
      return;
    }
    try {
      if (selectedWorkspace?.folderPath && caseId && operationName) {
        await connectNetAppFolder({
          operationName: operationName,
          folderPath: selectedWorkspace?.folderPath,
          caseId: caseId,
        });
        navigate(`/case/${caseId}/case-management`);
      }
    } catch (error) {
      if (error)
        navigate(`/case/${caseId}/netapp-connect/error`, {
          state: {
            isRouteValid: true,
            backLinkUrl,
          },
        });
    }
  };
  return (
    <div className={styles.confirmationWrapper}>
      <BackLink to={backLinkUrl} state={{ isRouteValid: true }}>
        Back
      </BackLink>
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
                      Confirm you want to link{" "}
                      <b>
                        &quot;
                        {getFolderNameFromPath(
                          selectedWorkspace?.folderPath || "",
                        )}
                        &quot;
                      </b>{" "}
                      Shared Drive folder to the case?
                    </span>
                  </>
                ),
              },
            }}
            name="Are you sure?"
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
