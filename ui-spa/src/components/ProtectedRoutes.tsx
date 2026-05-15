import { Outlet, Navigate, useLocation } from "react-router-dom";

const ProtectedRoutes = () => {
  const { state } = useLocation();

  const isAllowed = state?.isRouteValid;
  return isAllowed ? <Outlet /> : <Navigate to="/" replace />;
};

export default ProtectedRoutes;
