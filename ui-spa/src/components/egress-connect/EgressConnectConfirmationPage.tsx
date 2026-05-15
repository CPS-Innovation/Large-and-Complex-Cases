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
      searchQueryString: string;
      selectedWorkspace: {
        name: string;
        id: string;
      };
      backLinkUrl: string;
    };
  } = useLocation();

  const {
    caseId,
    backLinkUrl,
    selectedWorkspace,
    searchQueryString,
    isNetAppConnected,
  } = state || {};
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
          caseId: caseId,
        });

        if (!isNetAppConnected) {
          navigate(
            `/case/${caseId}/netapp-connect?operation-name=${selectedWorkspace.name}`,
            {
              state: {
                isRouteValid: true,
                searchQueryString: searchQueryString,
              },
            },
          );
          return;
        }
        navigate(`/case/${caseId}/case-management`);
      }
    } catch (error) {
      if (error) {
        navigate(`/case/${caseId}/egress-connect/error`, {
          state: {
            isRouteValid: true,
            backLinkUrl,
            searchQueryString,
            isNetAppConnected,
          },
        });
      }
    }
  };

  return (
    <div className={styles.confirmationWrapper}>
      <BackLink
        to={backLinkUrl}
        state={{ isRouteValid: true, searchQueryString, isNetAppConnected }}
      >
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
