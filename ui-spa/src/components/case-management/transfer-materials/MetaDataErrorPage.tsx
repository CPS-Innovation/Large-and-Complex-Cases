import { useMemo } from "react";
import { Button, LinkButton, BackLink } from "../../govuk";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { EgressConnectRouteState } from "../../../common/types/EgressConnectRouteState";
import { SharedDriveConnectRouteState } from "../../../common/types/SharedDriveConnectRouteState";
import { getUrlSearchParam } from "../../../common/utils/getUrlSearchParam";
import styles from "./MetaDataErrorPage.module.scss";

const MetaDataErrorPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId } = useParams();
  const searchParams = new URLSearchParams(location.search);
  const operationName = searchParams.get("operation-name");
  const errorType = useMemo(() => {
    return location.pathname.includes("egress-connection-error")
      ? "egress"
      : "shareddrive";
  }, [location]);
  const handleCancel = () => {
    navigate("/");
  };
  const handleReconnect = () => {
    if (errorType === "egress") {
      const payload: EgressConnectRouteState = {
        isRouteValid: true,
        searchQueryString: "",
        isNetAppConnected: true,
      };
      return navigate(
        `/case/${caseId}/egress-connect?${getUrlSearchParam("workspace-name", operationName)}`,
        {
          state: payload,
        },
      );
    }

    const payload: SharedDriveConnectRouteState = {
      isRouteValid: true,
      searchQueryString: "",
      netappRootFolderPath: "",
    };
    return navigate(
      `/case/${caseId}/netapp-connect?${getUrlSearchParam("operation-name", operationName)}`,
      {
        state: payload,
      },
    );
  };

  return (
    <div>
      <BackLink to={"/"}>Back</BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">
          There is a problem connecting to{" "}
          {errorType === "shareddrive" ? "the Shared Drive" : "Egress"}
        </h1>
        {errorType === "shareddrive" ? (
          <p>
            The connection to the Shared Drive folder for the{" "}
            <b>{operationName}</b> case has stopped working.
          </p>
        ) : (
          <p>
            The connection to the Egress case for <b>{operationName}</b> has
            stopped working.
          </p>
        )}

        <div className={styles.btnWrapper}>
          <Button onClick={handleReconnect}>Reconnect</Button>
          <LinkButton onClick={handleCancel}>Cancel</LinkButton>
        </div>
      </div>
    </div>
  );
};

export default MetaDataErrorPage;
