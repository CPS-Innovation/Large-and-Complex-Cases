import { useState, useEffect, useMemo, useContext } from "react";
import { Tabs } from "../common/tabs/Tabs";
import { TabId } from "../../common/types/CaseManagement";
import { ItemProps } from "../common/tabs/types";
import TransferMaterialsPage from "./transfer-materials";
import TransferMaterialsV1Page from "./transfer-materials-v1";
import TransferResolveFilePathPage from "./transfer-materials/TransferResolveFilePathPage";
import ActivityLogPage from "./activity-log/index";
import { getCaseMetaData } from "../../apis/gateway-api";
import { useNavigate, useLocation, useParams } from "react-router-dom";
import { MainStateContext } from "../../providers/MainStateProvider";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import TransferTreeViewPage from "../case-management/transfer-materials/TransferTreeViewPage";
import { useQuery } from "@tanstack/react-query";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";

import styles from "./index.module.scss";

const CaseManagementPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const {
    state: routeState,
  }: {
    state?: {
      transferId: string;
      transferSource: "egress" | "netapp";
      transferEgressFolderPathInitialValue?: string;
      transferNetAppFolderPathInitialValue?: string;
    };
  } = location;
  const { caseId } = useParams() as { caseId: string };
  if (!caseId) throw new Error("missing caseId in the url");

  const [activeTabId, setActiveTabId] = useState<TabId>("transfer-materials");

  const { state, dispatch } = useContext(MainStateContext);
  const {
    appData: { featureFlags },
  } = state;
  const handleTabSelection = (tabId: TabId) => {
    setActiveTabId(tabId);
  };

  const { data: caseMetaData, isLoading: isCaseMetaDataLoading } = useQuery({
    queryKey: [caseId],
    queryFn: () => getCaseMetaData(caseId),
    retry: false,
    enabled: true,
    throwOnError: true,
    staleTime: 0,
    gcTime: 0,
  });
  const operationNameOrDefendantName = useMemo(() => {
    return caseMetaData?.operationName || caseMetaData?.leadDefendantName || "";
  }, [caseMetaData]);

  useEffect(() => {
    if (caseMetaData) {
      dispatch({
        type: "SET_CASE_META_DATA",
        payload: {
          caseMetaData,
        },
      });
      if (!caseMetaData.egressWorkspaceId && !caseMetaData.netappFolderPath) {
        navigate("/");
      }
      if (!caseMetaData.egressWorkspaceId && caseMetaData.netappFolderPath) {
        navigate(
          `/case/${caseId}/case-management/egress-connection-error?${getUrlSearchParam("operation-name", operationNameOrDefendantName)}`,
          {
            state: {
              isRouteValid: true,
            },
          },
        );
      }
      if (caseMetaData.egressWorkspaceId && !caseMetaData.netappFolderPath) {
        navigate(
          `/case/${caseId}/case-management/shared-drive-connection-error?${getUrlSearchParam("operation-name", operationNameOrDefendantName)}`,
          {
            state: {
              isRouteValid: true,
            },
          },
        );
      }
    }
  }, [caseMetaData, navigate, caseId, operationNameOrDefendantName, dispatch]);

  const tabItems = useMemo(() => {
    const items: ItemProps<TabId>[] = [];

    if (featureFlags?.transferMaterialsV1) {
      items.push({
        id: "transfer-materials",
        label: "Transfer materials",
        panel: {
          children: caseMetaData ? (
            <TransferMaterialsV1Page
              isTabActive={activeTabId === "transfer-materials"}
              caseId={caseId}
              operationName={operationNameOrDefendantName}
              egressWorkspaceId={caseMetaData.egressWorkspaceId}
              netAppPath={caseMetaData.netappFolderPath}
              activeTransferId={
                routeState?.transferId ?? caseMetaData.activeTransferId ?? ""
              }
              urn={caseMetaData.urn}
              transferSourceInitialValue={
                routeState?.transferSource ?? "egress"
              }
              transferEgressFolderPathInitialValue={
                routeState?.transferEgressFolderPathInitialValue ?? null
              }
              transferNetAppFolderPathInitialValue={
                routeState?.transferNetAppFolderPathInitialValue ?? null
              }
            />
          ) : (
            <></>
          ),
        },
      });
    }

    if (featureFlags !== null && !featureFlags?.transferMaterialsV1) {
      items.push({
        id: "transfer-materials",
        label: "Transfer materials",
        panel: {
          children: caseMetaData ? (
            <TransferMaterialsPage
              isTabActive={activeTabId === "transfer-materials"}
              caseId={caseId}
              operationName={operationNameOrDefendantName}
              egressWorkspaceId={caseMetaData.egressWorkspaceId}
              netAppPath={caseMetaData.netappFolderPath}
              activeTransferId={
                routeState?.transferId ?? caseMetaData.activeTransferId ?? ""
              }
              urn={caseMetaData.urn}
            />
          ) : (
            <></>
          ),
        },
      });
    }

    items.push({
      id: "activity-log",
      label: "Activity log",
      panel: {
        children: caseMetaData ? (
          <div>
            <ActivityLogPage
              operationName={operationNameOrDefendantName}
              isTabActive={activeTabId === "activity-log"}
            />
          </div>
        ) : (
          <></>
        ),
      },
    });
    if (featureFlags?.caseDetails) {
      items.push({
        id: "case-details",
        label: "Case Details",
        panel: {
          children: caseMetaData ? (
            <div>
              <h3> Case Details</h3>
              <TransferTreeViewPage caseId={caseId} />
            </div>
          ) : (
            <></>
          ),
        },
      });
    }
    return items;
  }, [
    activeTabId,
    caseId,
    caseMetaData,
    routeState,
    featureFlags,
    operationNameOrDefendantName,
  ]);
  if (isCaseMetaDataLoading) {
    return (
      <PageContentWrapper>
        <div aria-live="polite">Loading...</div>
      </PageContentWrapper>
    );
  }
  if (
    location.pathname.endsWith("/transfer-resolve-file-path") ||
    location.pathname.endsWith("/transfer-rename-file")
  )
    return <TransferResolveFilePathPage />;

  return (
    <PageContentWrapper>
      <h1 className={styles.workspaceName}>{operationNameOrDefendantName}</h1>
      <div className={styles.urnText} data-testid="case-urn">
        <span>{caseMetaData?.urn}</span>
      </div>
      <Tabs
        items={tabItems.map((item) => ({
          id: item.id,
          label: item.label,
          panel: item.panel,
        }))}
        activeTabId={activeTabId}
        handleTabSelection={handleTabSelection}
      />
    </PageContentWrapper>
  );
};

export default CaseManagementPage;
