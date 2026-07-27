import { Routes, Route, Navigate } from "react-router";
import { useEffect, useContext } from "react";
import CaseSearchPage from "./search-page";
import CaseSearchResultPage from "./search-result-page";
import EgressPage from "./egress-connect";
import EgressConnectConfirmationPage from "./egress-connect/EgressConnectConfirmationPage";
import EgressConnectFailurePage from "./egress-connect/EgressConnectFailurePage";
import NetAppPage from "./netapp-connect";
import NetAppConnectConfirmationPage from "./netapp-connect/NetAppConnectConfirmationPage";
import NetAppConnectFailurePage from "./netapp-connect/NetAppConnectFailurePage";
import CaseManagementPage from "./case-management";
import MetaDataErrorPage from "./case-management/transfer-materials/MetaDataErrorPage";
import FolderAccessErrorPage from "./case-management/transfer-materials/FolderAccessErrorPage";
import TransferErrorPage from "./case-management/transfer-materials/TransferErrorPage";
import TransferMovePermissionsErrorPage from "./case-management/transfer-materials/TransferMovePermissionsErrorPage";
import DisconnectSharedDriveConfirmationPage from "./case-management/netapp-disconnect/DisconnectSharedDriveConfirmationPage";
import DisconnectSharedDriveSuccessPage from "./case-management/netapp-disconnect/DisconnectSharedDriveSuccessPage";
import DisconnectSharedDriveFailurePage from "./case-management/netapp-disconnect/DisconnectSharedDriveFailurePage";
import TransferDestinationPage from "./case-management/transfer-materials-v1/TransferDestinationPage";
import { MainStateContext } from "../providers/MainStateProvider";
import { useUserGroupsFeatureFlag } from "../common/hooks/useUserGroupsFeatureFlag";

import ProtectedRoutes from "./ProtectedRoutes";

const AppRoutes = () => {
  const { state, dispatch } = useContext(MainStateContext);
  const { appData: { featureFlags } = {} } = state;
  const featureFlagsData = useUserGroupsFeatureFlag();
  useEffect(() => {
    //set feature flag data, when there is a valid value and  when the featureFlags are not set.
    if (featureFlagsData && !featureFlags) {
      dispatch({
        type: "SET_FEATURE_FLAGS",
        payload: {
          featureFlags: featureFlagsData,
        },
      });
    }
  }, [featureFlagsData, featureFlags, dispatch]);

  return (
    <Routes>
      <Route index element={<CaseSearchPage />} />
      <Route path="/search-results" element={<CaseSearchResultPage />} />
      <Route
        path="/case/:caseId/case-management"
        element={<CaseManagementPage />}
      />

      <Route element={<ProtectedRoutes />}>
        <Route
          path="/case/:caseId/case-management/transfer-resolve-file-path"
          element={<CaseManagementPage />}
        />
        <Route
          path="/case/:caseId/case-management/transfer-rename-file"
          element={<CaseManagementPage />}
        />
        <Route
          path="/case/:caseId/case-management/disconnect-shared-drive-confirmation"
          element={<DisconnectSharedDriveConfirmationPage />}
        />
        <Route
          path="/case/:caseId/case-management/disconnect-shared-drive-success"
          element={<DisconnectSharedDriveSuccessPage />}
        />
        <Route
          path="/case/:caseId/case-management/disconnect-shared-drive-failure"
          element={<DisconnectSharedDriveFailurePage />}
        />
        <Route path="/case/:caseId/egress-connect" element={<EgressPage />} />
        <Route
          path="/case/:caseId/egress-connect/confirmation"
          element={<EgressConnectConfirmationPage />}
        />
        <Route
          path="/case/:caseId/egress-connect/error"
          element={<EgressConnectFailurePage />}
        />
        <Route path="/case/:caseId/netapp-connect" element={<NetAppPage />} />
        <Route
          path="/case/:caseId/netapp-connect/confirmation"
          element={<NetAppConnectConfirmationPage />}
        />
        <Route
          path="/case/:caseId/netapp-connect/error"
          element={<NetAppConnectFailurePage />}
        />
        <Route
          path="/case/:caseId/case-management/egress-connection-error"
          element={<MetaDataErrorPage />}
        />
        <Route
          path="/case/:caseId/case-management/shared-drive-connection-error"
          element={<MetaDataErrorPage />}
        />
        <Route
          path="/case/:caseId/case-management/transfer-errors"
          element={<TransferErrorPage />}
        />
        <Route
          path="/case/:caseId/case-management/transfer-permissions-error"
          element={<TransferMovePermissionsErrorPage />}
        />
        <Route
          path="/case/:caseId/case-management/connection-error"
          element={<FolderAccessErrorPage />}
        />
        <Route
          path="/case/:caseId/case-management/transfer-destination-page"
          element={<TransferDestinationPage />}
        />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

export default AppRoutes;
