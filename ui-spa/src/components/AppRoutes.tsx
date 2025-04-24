import { Routes, Route } from "react-router";
import CaseSearchPage from "./search-page";
import CaseSearchResultPage from "./search-result-page";
import EgressPage from "./egress-connect";
import NetAppPage from "./netapp-connect";

const AppRoutes = () => {
  return (
    <Routes>
      <Route index element={<CaseSearchPage />} />
      <Route path="/search-results" element={<CaseSearchResultPage />} />
      <Route path="/case/:caseId/egress-connect" element={<EgressPage />} />
      <Route path="/case/:caseId/netapp-connect" element={<NetAppPage />} />
      <Route
        path="/case/:caseId/egress-connect/confirmation"
        element={<EgressPage />}
      />
      <Route
        path="/case/:caseId/egress-connect/error"
        element={<EgressPage />}
      />
      <Route
        path="/case/:caseId/netapp-connect/confirmation"
        element={<NetAppPage />}
      />
      <Route
        path="/case/:caseId/netapp-connect/error"
        element={<NetAppPage />}
      />
      {/* <Route path="*" element={<Navigate to="/" replace />} /> */}
    </Routes>
  );
};

export default AppRoutes;
