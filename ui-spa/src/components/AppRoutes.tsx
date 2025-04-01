import { Routes, Route } from "react-router";
import CaseSearchPage from "./search-page";
import CaseSearchResultPage from "./search-result-page";
import EgressPage from "./egress-connect";

const AppRoutes = () => {
  return (
    <Routes>
      <Route index element={<CaseSearchPage />} />
      <Route path="search-results" element={<CaseSearchResultPage />} />
      <Route path="/egress-connect" element={<EgressPage />} />
    </Routes>
  );
};

export default AppRoutes;
