import { Routes, Route } from "react-router";
import CaseSearchPage from "./search-page";
import CaseSearchResultPage from "./search-result-page";

const AppRoutes = () => {
  return (
    <Routes>
      <Route index element={<CaseSearchPage />} />
      <Route path="search-results" element={<CaseSearchResultPage />} />
    </Routes>
  );
};

export default AppRoutes;
