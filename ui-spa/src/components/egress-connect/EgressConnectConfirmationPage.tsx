import { useState } from "react";
import { Button, Radios, BackLink } from "../govuk";
import { useLocation, useNavigate } from "react-router-dom";
import { connectEgressWorkspace } from "../../apis/gateway-api";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import styles from "./EgressConnectConfirmationPage.module.scss";

const EgressConnectConfirmationPage: React.FC = () => {
  const {
    state,
  }: {
    state?: {
      caseId: string;
      isNetAppConnected: boolean;
      selectedWorkspace: {
        name: string;
        id: string;
      };
      backLinkUrl: string;
    };
  } = useLocation();

  const { caseId, backLinkUrl, selectedWorkspace } = state || {};
  const [formValue, setFormValue] = useState("yes");
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
      if (state?.selectedWorkspace.id && state?.caseId) {
        await connectEgressWorkspace({
          workspaceId: state?.selectedWorkspace.id,
          caseId: state?.caseId,
        });

        if (!state?.isNetAppConnected)
          navigate(
            `/case/${state?.caseId}/netapp-connect?operation-name=${state?.selectedWorkspace.name}`,
            {
              state: {
                searchQueryString: "",
              },
            },
          );
        else navigate(`/case/${caseId}/case-management`);
      }
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
    } catch (e) {
      navigate(`/case/${caseId}/egress-connect/error`, {
        state: {
          isRouteValid: true,
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
                      <b>&quot;{selectedWorkspace?.name}&quot;</b> on Egress to
                      the case?
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
      </PageContentWrapper>
    </div>
  );
};

export default EgressConnectConfirmationPage;
