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
      <ErrorBoundary fallbackRender={ErrorBoundaryFallback}>
        <QueryClientProvider client={queryClient}>
          <Auth>
            <MainStateProvider>
              <Layout>
                <AppRoutes />
              </Layout>
            </MainStateProvider>
          </Auth>
        </QueryClientProvider>
      </ErrorBoundary>
    </BrowserRouter>
  );
}

export default App;
