import { Routes, Route } from "react-router";
import CaseSearchPage from "../components/search-page";
import CaseSearchResultPage from "../components/search-result-page";

const AppRoutes = () => {
  return (
    <Routes>
      <Route index element={<CaseSearchPage />} />
      <Route path="search-results" element={<CaseSearchResultPage />} />
    </Routes>
  );
};

export default AppRoutes;
