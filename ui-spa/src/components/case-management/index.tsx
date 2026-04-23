import { useState, useEffect, useCallback, useMemo } from "react";
import { Tabs } from "../common/tabs/Tabs";
import { TabId } from "../../common/types/CaseManagement";
import { ItemProps } from "../common/tabs/types";
import TransferMaterialsPage from "./transfer-materials";
import TransferResolveFilePathPage from "./transfer-materials/TransferResolveFilePathPage";
import ActivityLogPage from "./activity-log/index";
import { useApi } from "../../common/hooks/useApi";
import { getCaseMetaData } from "../../apis/gateway-api";
import { useNavigate, useLocation, useParams } from "react-router-dom";
import { useUserGroupsFeatureFlag } from "../../common/hooks/useUserGroupsFeatureFlag";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import TransferWidget from "../common/transfer-widget/TransferWidget";
import { getNetAppFolders, getEgressFolders } from "../../apis/gateway-api";
import { getFolderNameFromPath } from "../../common/utils/getFolderNameFromPath";

import styles from "./index.module.scss";

const CaseManagementPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId } = useParams() as { caseId: string };
  if (!caseId) throw new Error("missing caseId in the url");

  const caseMetaData = useApi(getCaseMetaData, [caseId], true);

  const {
    refetch: netAppRefetch,
    status: netAppStatus,
    data: netAppData,
    error: netAppError,
  } = useApi(getNetAppFolders, [caseMetaData.data?.netappFolderPath], false);

  const {
    refetch: egressRefetch,
    status: egressStatus,
    data: egressData,
    error: egressError,
  } = useApi(
    getEgressFolders,
    [caseMetaData.data?.egressWorkspaceId, ""],
    false,
  );

  useEffect(() => {
    if (caseMetaData.data?.egressWorkspaceId) {
      egressRefetch();
    }
  }, [caseMetaData.data?.egressWorkspaceId, egressRefetch]);

  useEffect(() => {
    if (caseMetaData.data?.netappFolderPath) {
      netAppRefetch();
    }
  }, [caseMetaData.data?.netappFolderPath, netAppRefetch]);

  const initialEgressFolderData = useMemo(() => {
    if (!egressData) return [];
    const folders = egressData
      .filter((folder) => folder.isFolder)
      .map((folder) => {
        return {
          id: folder.id,
          name: folder.name,
          path: folder.path,
          isFolder: true,
        };
      });
    return folders;
  }, [egressData]);

  const initialNetappFolderData = useMemo(() => {
    if (!caseMetaData?.data) return [];
    const folders = [
      {
        id: caseMetaData.data?.netappFolderPath,
        name: getFolderNameFromPath(caseMetaData.data?.netappFolderPath),
        path: caseMetaData.data?.netappFolderPath,
        isFolder: true,
      },
    ];

    return folders;
  }, [caseMetaData]);
  const [activeTabId, setActiveTabId] = useState<TabId>("transfer-materials");

  const featureFlags = useUserGroupsFeatureFlag();
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
            isTabActive={activeTabId === "transfer-materials"}
            caseId={caseId}
            operationName={caseMetaData.data.operationName}
            egressWorkspaceId={caseMetaData.data.egressWorkspaceId}
            netAppPath={caseMetaData?.data.netappFolderPath}
            activeTransferId={
              location?.state?.transferId ?? caseMetaData?.data.activeTransferId
            }
          />
        ) : (
          <></>
        ),
      },
    },
    {
      id: "activity-log",
      label: "Activity log",
      panel: {
        children: caseMetaData?.data ? (
          <div>
            <ActivityLogPage
              operationName={caseMetaData.data.operationName}
              isTabActive={activeTabId === "activity-log"}
            />
          </div>
        ) : (
          <></>
        ),
      },
    },
  ];

  if (featureFlags.caseDetails) {
    items.push({
      id: "case-details",
      label: "Case Details",
      panel: {
        children: caseMetaData?.data ? (
          <div>
            <h3> Case Details</h3>
            <TransferWidget
              data={initialNetappFolderData}
              onLoadChildren={async (nodeId) => {
                console.log("Load children for node:", nodeId);
                await new Promise((res) => setTimeout(res, 1000));

                const data = await getNetAppFolders(nodeId);

                const folders = data.folderData.map((folder) => {
                  return {
                    id: folder.path,
                    name: getFolderNameFromPath(folder.path),
                    path: folder.path,
                    isFolder: true,
                  };
                });
                return folders;
              }}
              transferAction="Copy"
            />
          </div>
        ) : (
          <></>
        ),
      },
    });
  }
  if (caseMetaData.status === "loading" || caseMetaData.status === "initial") {
    return <PageContentWrapper>loading...</PageContentWrapper>;
  }
  if (
    location.pathname.endsWith("/transfer-resolve-file-path") ||
    location.pathname.endsWith("/transfer-rename-file")
  )
    return <TransferResolveFilePathPage />;

  return (
    <PageContentWrapper>
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
        activeTabId={activeTabId}
        handleTabSelection={handleTabSelection}
      />
    </PageContentWrapper>
  );
};

export default CaseManagementPage;
