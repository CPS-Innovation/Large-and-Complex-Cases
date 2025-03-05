import { Routes, Route } from "react-router";
import CaseSearchPage from "../components/search-page";
import CaseSearchResultPage from "../components/search-result-page";

const AppRoutes = () => {
  return (
    <Routes>
      <Route index element={<CaseSearchPage />} />
      <Route path="cases" element={<CaseSearchResultPage />} />
    </Routes>
  );
};

export default AppRoutes;
