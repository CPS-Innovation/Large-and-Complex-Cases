import { useState, useEffect, useCallback } from "react";
import { Tabs } from "../common/tabs/Tabs";
import { TabId } from "../../common/types/CaseManagement";
import { ItemProps } from "../common/tabs/types";
import TransferMaterialsPage from "./transfer-materials";
import TransferResolveFilePathPage from "./transfer-materials/TransferResolveFilePathPage";
import { useApi } from "../../common/hooks/useApi";
import { getCaseMetaData } from "../../apis/gateway-api";
import { useNavigate, useLocation, useParams } from "react-router-dom";

import styles from "./index.module.scss";

const CaseManagementPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId } = useParams() as { caseId: string };
  if (!caseId) throw new Error("missing caseId in the url");

  const caseMetaData = useApi(getCaseMetaData, [caseId], true);

  const [activeTabId, setActiveTabId] = useState<TabId>("transfer-materials");
  const handleTabSelection = (tabId: TabId) => {
    setActiveTabId(tabId);
  };

  useEffect(() => {
    if (caseMetaData.status === "failed")
      throw new Error(`${caseMetaData.error}`);
    if (caseMetaData.status === "succeeded") {
      if (
        !caseMetaData.data?.egressWorkspaceId &&
        !caseMetaData.data?.netappFolderPath
      ) {
        navigate("/");
      }
      if (
        !caseMetaData.data?.egressWorkspaceId &&
        caseMetaData.data?.netappFolderPath
      ) {
        navigate(
          `/case/${caseId}/case-management/egress-connection-error?operation-name=${caseMetaData.data?.operationName}`,
          {
            state: {
              isValid: true,
            },
          },
        );
      }
      if (
        caseMetaData.data?.egressWorkspaceId &&
        !caseMetaData.data?.netappFolderPath
      ) {
        navigate(
          `/case/${caseId}/case-management/shared-drive-connection-error?operation-name=${caseMetaData.data?.operationName}`,
          {
            state: {
              isValid: true,
            },
          },
        );
      }
    }
  }, [caseMetaData, navigate, caseId]);

  const validateRoute = useCallback(() => {
    if (
      location.pathname.endsWith("/transfer-resolve-file-path") &&
      !location?.state?.isRouteValid
    ) {
      navigate(`/`);
    }
    if (
      location.pathname.endsWith("/transfer-rename-file") &&
      !location?.state?.isRouteValid
    ) {
      navigate(`/`);
    }
    if (
      location.pathname.endsWith("/transfer-errors") &&
      !location?.state?.isRouteValid
    ) {
      navigate(`/`);
    }
  }, [location, navigate]);

  useEffect(() => {
    validateRoute();
  }, [location, validateRoute]);

  const items: ItemProps<TabId>[] = [
    {
      id: "transfer-materials",
      label: "Transfer materials",
      panel: {
        children: caseMetaData?.data ? (
          <TransferMaterialsPage
            caseId={caseId}
            operationName={caseMetaData.data.operationName}
            egressWorkspaceId={caseMetaData.data.egressWorkspaceId}
            netAppPath={caseMetaData?.data.netappFolderPath}
            activeTransferId={
              location?.state?.transferId ?? caseMetaData?.data.activeTransferId
            }
          />
        ) : (
          <div> </div>
        ),
      },
    },
    {
      id: "manage-materials",
      label: "Manage materials",
      panel: { children: <div>manage materials</div> },
    },
  ];
  if (caseMetaData.status === "loading" || caseMetaData.status === "initial") {
    return <div className="govuk-width-container">loading...</div>;
  }
  if (
    location.pathname.endsWith("/transfer-resolve-file-path") ||
    location.pathname.endsWith("/transfer-rename-file")
  )
    return <TransferResolveFilePathPage />;
  if (location.pathname.endsWith("/transfer-errors"))
    return (
      <div className="govuk-width-container">
        <div>
          <h1>Handle Transfer errors</h1>
        </div>
      </div>
    );
  return (
    <div className="govuk-width-container">
      <h1 className={styles.workspaceName}>
        {caseMetaData?.data?.operationName}
      </h1>
      <div className={styles.urnText}>
        <span>{caseMetaData?.data?.urn}</span>
      </div>

      <Tabs
        items={items.map((item) => ({
          id: item.id,
          label: item.label,
          panel: item.panel,
        }))}
        title="Contents"
        activeTabId={activeTabId}
        handleTabSelection={handleTabSelection}
      />
    </div>
  );
};

export default CaseManagementPage;
