import { useState } from "react";
import { Button, Radios, BackLink } from "../govuk";
import { useLocation, useNavigate } from "react-router-dom";
import { connectEgressWorkspace } from "../../apis/gateway-api";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import { SharedDriveConnectRouteState } from "../../common/types/SharedDriveConnectRouteState";
import { EgressConnectConfirmationRouteState } from "../../common/types/EgressConnectConfirmationRouteState";
import { EgressConnectRouteState } from "../../common/types/EgressConnectRouteState";
import { EgressConnectFailureRouteState } from "../../common/types/EgressConnectFailureRouteState";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import styles from "./EgressConnectConfirmationPage.module.scss";

const EgressConnectConfirmationPage: React.FC = () => {
  const {
    state,
  }: {
    state: EgressConnectConfirmationRouteState;
  } = useLocation();

  const {
    caseId,
    backLinkUrl,
    selectedWorkspace,
    searchQueryString,
    isNetAppConnected,
  } = state;
  const [formValue, setFormValue] = useState("yes");
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (formValue === "no" && backLinkUrl) {
      navigate(backLinkUrl, {
        state: {
          isRouteValid: true,
          searchQueryString,
          isNetAppConnected,
        },
      });
      return;
    }
    try {
      if (selectedWorkspace?.id && caseId) {
        await connectEgressWorkspace({
          workspaceId: selectedWorkspace.id,
          workspaceName: selectedWorkspace.name,
          caseId: caseId,
        });

        if (!isNetAppConnected) {
          const payload: SharedDriveConnectRouteState = {
            isRouteValid: true,
            searchQueryString: searchQueryString,
            netappRootFolderPath: "",
          };
          navigate(
            `/case/${caseId}/netapp-connect?${getUrlSearchParam("operation-name", selectedWorkspace.name)}`,
            {
              state: payload,
            },
          );
          return;
        }
        navigate(`/case/${caseId}/case-management`);
      }
    } catch (error) {
      if (error) {
        const payload: EgressConnectFailureRouteState = {
          isRouteValid: true,
          backLinkUrl,
          searchQueryString,
          isNetAppConnected,
        };
        navigate(`/case/${caseId}/egress-connect/error`, {
          state: payload,
        });
      }
    }
  };

  const backLinkPayload: EgressConnectRouteState = {
    isRouteValid: true,
    searchQueryString,
    isNetAppConnected,
  };
  return (
    <div className={styles.confirmationWrapper}>
      <BackLink to={backLinkUrl} state={backLinkPayload}>
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
