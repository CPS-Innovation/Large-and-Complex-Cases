import { useState, useContext } from "react";
import { Button, Radios, BackLink } from "../govuk";
import { useNavigate, useParams } from "react-router-dom";
import { connectEgressWorkspace } from "../../apis/gateway-api";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import { MainStateContext } from "../../providers/MainStateProvider";
import styles from "./EgressConnectConfirmationPage.module.scss";

const EgressConnectConfirmationPage: React.FC = () => {
  const { state, dispatch } = useContext(MainStateContext);
  const {
    backLinkUrl,
    selectedWorkspace,
    searchQueryString,
    isNetAppConnected,
  } = state.appData.egressConnectConfirmationPage;
  const { caseId } = useParams() as { caseId: string };
  const [formValue, setFormValue] = useState("yes");
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (formValue === "no" && backLinkUrl) {
      navigate(backLinkUrl);
      return;
    }
    try {
      if (selectedWorkspace.id && caseId) {
        await connectEgressWorkspace({
          workspaceId: selectedWorkspace.id,
          workspaceName: selectedWorkspace.name,
          caseId: caseId,
        });

        if (!isNetAppConnected) {
          dispatch({
            type: "SET_SHARED_DRIVE_CONNECT_PAGE",
            payload: {
              searchQueryString,
              netappRootFolderPath: "",
            },
          });
          navigate(
            `/case/${caseId}/netapp-connect?${getUrlSearchParam("operation-name", selectedWorkspace.name)}`,
          );
          return;
        }
        navigate(`/case/${caseId}/case-management`);
      }
    } catch (error) {
      if (error) {
        dispatch({
          type: "SET_EGRESS_CONNECT_FAILURE_PAGE",
          payload: { backLinkUrl },
        });
        navigate(`/case/${caseId}/egress-connect/error`);
      }
    }
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
