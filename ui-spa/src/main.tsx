import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./components/App.tsx";
import { MOCK_API_SOURCE } from "./config.ts";
import "./index.scss";

async function enableMocking() {
  if (MOCK_API_SOURCE !== "dev") {
    return;
  }

  const { setupMockApi } = await import("./mocks/browser");

  // `worker.start()` returns a Promise that resolves
  // once the Service Worker is up and ready to intercept requests.
  return setupMockApi({
    baseUrl: GATEWAY_BASE_URL,
    sourceName: MOCK_API_SOURCE,
  });
}

enableMocking().then(() => {
  createRoot(document.getElementById("root")!).render(
    <StrictMode>
      <App />,
    </StrictMode>,
  );
});
