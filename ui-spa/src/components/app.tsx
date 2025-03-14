import Layout from "./layout";
import { Auth } from "../auth";
import { BrowserRouter } from "react-router";
import AppRoutes from "../components/routes";

import { MainStateProvider } from "../providers/mainStateProvider";
import { ErrorBoundary } from "react-error-boundary";
import { ErrorBoundaryFallback } from "./ErrorBoundaryFallback";

function App() {
  return (
    <BrowserRouter>
      <ErrorBoundary fallbackRender={ErrorBoundaryFallback}>
        <Auth>
          <MainStateProvider>
            <Layout>
              <AppRoutes />
            </Layout>
          </MainStateProvider>
        </Auth>
      </ErrorBoundary>
    </BrowserRouter>
  );
}

export default App;
