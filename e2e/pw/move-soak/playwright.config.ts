import { defineConfig, devices } from "@playwright/test";
import dotenv from "dotenv";
import path from "node:path";
import { randomUUID } from "node:crypto";

const runId = randomUUID();

dotenv.config({
  path: path.resolve(__dirname, `.env.${process.env.ENVIRONMENT || "local"}`),
});

export default defineConfig({
  testDir: "./tests",
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,
  reporter: [
    ["list"], 
["html", { outputFolder: `./playwright-report/${runId}` }],
    ["junit", { outputFile: `./playwright-report/e2e-test-report-${runId}.xml` }],
  ],
  timeout: 120_000,
  expect: { timeout: 500_000 },
  use: {
    baseURL: process.env.BASE_URL,
    headless: true,
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
  },
});
