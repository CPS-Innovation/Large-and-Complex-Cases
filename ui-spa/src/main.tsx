import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import { MOCK_API_SOURCE } from "./config.ts";
import "./index.scss";

async function enableMocking() {
  console.log("MOCK_API_SOURCE>>", MOCK_API_SOURCE);
  if (!MOCK_API_SOURCE || process.env.NODE_ENV !== "development") {
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
