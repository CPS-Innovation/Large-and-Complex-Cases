import { useContext } from "react";
import { Outlet, Navigate } from "react-router-dom";
import { MainStateContext } from "../providers/MainStateProvider";

const ProtectedRoutes = () => {
  const { state } = useContext(MainStateContext);
  const isAllowed =
    state.apiData.caseDivisionsOrAreas || state.apiData.caseMetaData;
  return isAllowed ? <Outlet /> : <Navigate to="/" replace />;
};

export default ProtectedRoutes;
