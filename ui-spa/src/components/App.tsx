import Layout from "./Layout";
import { Auth } from "../auth";
import { BrowserRouter } from "react-router";
import AppRoutes from "./AppRoutes";

import { MainStateProvider } from "../providers/MainStateProvider";
import { ErrorBoundary } from "react-error-boundary";
import { ErrorBoundaryFallback } from "./ErrorBoundaryFallback";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
const queryClient = new QueryClient();

function App() {
  return (
    <BrowserRouter>
      <MainStateProvider>
        <ErrorBoundary fallbackRender={ErrorBoundaryFallback}>
          <QueryClientProvider client={queryClient}>
            <Auth>
              <Layout>
                <AppRoutes />
              </Layout>
            </Auth>
          </QueryClientProvider>
        </ErrorBoundary>
      </MainStateProvider>
    </BrowserRouter>
  );
}

export default App;
