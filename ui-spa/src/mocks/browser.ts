import { setupWorker } from "msw/browser";
import { setupHandlers } from "./handlers";

export type MockApiConfig = {
  baseUrl: string;
  sourceName: "playwright" | "dev";
};

export const setupMockApi = async (config: MockApiConfig) => {
  const worker = setupWorker(...setupHandlers(config));
  return await worker.start();
};
