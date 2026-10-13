import { Panel } from "../../govuk";
import { useContext, useMemo } from "react";
import { Link, useLocation } from "react-router";
import { MainStateContext } from "../../../providers/MainStateProvider";
import styles from "./DisconnectSuccessPage.module.scss";
const DisconnectSuccessPage = () => {
  const { state } = useContext(MainStateContext);
  const location = useLocation();
  const { queryType } = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return {
      queryType: params.get("type") ?? "",
    };
  }, [location.search]);

  const { urn } = state.apiData.caseMetaData || {};
  return (
    <div className={styles.contentWrapper}>
      <Panel
        titleChildren={
          queryType === "shared-drive"
            ? "Shared Drive disconnected"
            : "Egress disconnected"
        }
      ></Panel>
      {queryType === "shared-drive" ? (
        <p>You&apos;ve disconnected the Shared Drive folder.</p>
      ) : (
        <p>You&apos;ve disconnected an Egress case.</p>
      )}
      {queryType === "shared-drive" ? (
        <p>You can connect a different folder if you need to.</p>
      ) : (
        <p>You can connect to another Egress case to transfer materials.</p>
      )}
      <Link
        to={`/search-results?urn=${urn}`}
        className="govuk-link--no-visited-state"
      >
        {queryType === "shared-drive" ? "Connect a folder" : "Connect Egress"}
      </Link>
    </div>
  );
};
export default DisconnectSuccessPage;
