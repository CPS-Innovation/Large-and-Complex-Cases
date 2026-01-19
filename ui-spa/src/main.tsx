import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./components/App.tsx";
import { MOCK_API_SOURCE, GATEWAY_BASE_URL } from "./config.ts";
import "./index.scss";

// Initialize CSP
const cspContent = `default-src 'self';
                    connect-src 'self' https://login.microsoftonline.com ${GATEWAY_BASE_URL};
                    style-src 'self' 'unsafe-inline';`;

const meta = document.createElement('meta');
meta.httpEquiv = 'Content-Security-Policy';
meta.content = cspContent;
document.head.appendChild(meta);

async function enableMocking() {
  // playwright test mock is handled separately through "playwright-msw"
  if (MOCK_API_SOURCE !== "dev") {
    return;
  }
  const { worker } = await import("./mocks/browser");
  // `worker.start()` returns a Promise that resolves
  // once the Service Worker is up and ready to intercept requests.
  return worker.start();
}

enableMocking().then(() => {
  createRoot(document.getElementById("root")!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  );
});