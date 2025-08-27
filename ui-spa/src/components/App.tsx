import Layout from "./Layout";
import { Auth } from "../auth";
import { BrowserRouter } from "react-router";
import AppRoutes from "./AppRoutes";

import { MainStateProvider } from "../providers/MainStateProvider";
import { ErrorBoundary } from "react-error-boundary";
import { ErrorBoundaryFallback } from "./ErrorBoundaryFallback";

function App() {
  return (
    <BrowserRouter>
      <ErrorBoundary fallbackRender={ErrorBoundaryFallback}>
        <Layout>
          <Auth>
            <MainStateProvider>
              <AppRoutes />
            </MainStateProvider>
          </Auth>
        </Layout>
      </ErrorBoundary>
    </BrowserRouter>
  );
}

export default App;
