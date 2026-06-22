import { defineConfig, devices } from "@playwright/test";
import dotenv from "dotenv";
import path from "node:path";

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
    ["html", { outputFolder: "./playwright-report" }],
    ["junit", { outputFile: "./playwright-report/e2e-test-report.xml" }]
  ],
  timeout: 120_000,
  expect: { timeout: 120_000 },
  use: {
    baseURL: process.env.BASE_URL,
    headless: true,
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
  },
  projects: [
    // Runs first. Creates one workspace, uploads a seed file, registers a
    // case, logs in, connects Egress + NetApp in the browser, and saves
    // storageState + shared case info for the register-case tests.
    // `teardown` runs after all dependent tests finish and deletes the
    // workspace so AUTOMATION-TESTING* workspaces don't accumulate.
    {
      name: "register-case-setup",
      testMatch: "**/register-case.setup.ts",
      teardown: "register-case-teardown",
      use: { ...devices["Desktop Chrome"] },
    },

    {
      name: "register-case-teardown",
      testMatch: "**/register-case.teardown.ts",
    },

    // Register-case specs reuse the connected case but re-do login per test.
    // Filename convention: *.spec.ts but NOT *-default.spec.ts.
    //
    // We intentionally do NOT apply the saved storageState here. Tactical
    // cookies captured at setup time age quickly; the LCC app leaves the
    // search radios disabled and rejects /api/v1/case-search with HTTP 400
    // when tactical is stale. The fixture runs the full tactical + AD login
    // each test, which is fast next to upload + transfer time, while still
    // skipping the one-time case register + Egress/NetApp connect done once
    // by the setup project.
    {
      name: "register-case-tests",
      testMatch: "**/*.spec.ts",
      testIgnore: "**/*-default.spec.ts",
      dependencies: ["register-case-setup"],
      use: { ...devices["Desktop Chrome"] },
    },

    // Default mode specs target a pre-existing connected case via
    // DEFAULT_WORKSPACE_ID / DEFAULT_CASE_URN and need no shared setup.
    {
      name: "default-mode-tests",
      testMatch: "**/*-default.spec.ts",
      use: { ...devices["Desktop Chrome"] },
    },

    // One-off project for seeding the canonical NetApp source fixture in
    // default mode. Run manually via:
    //   npx playwright test --project=seed-netapp-fixture
    // See README "Required NetApp fixture".
    {
      name: "seed-netapp-fixture",
      testMatch: "**/seed-netapp-fixture.setup.ts",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
});
