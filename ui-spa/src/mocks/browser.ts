import { setupWorker } from "msw/browser";
import { setupHandlers } from "./handlers";
import { GATEWAY_BASE_URL, MOCK_API_SOURCE } from "../config";

export type MockApiConfig = {
  baseUrl: string;
  sourceName: "playwright" | "dev";
};

export const worker = setupWorker(
  ...setupHandlers(GATEWAY_BASE_URL, MOCK_API_SOURCE),
);
