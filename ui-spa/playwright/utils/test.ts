import { test as base, expect } from "@playwright/test";
import { http } from "msw";
import type { MockServiceWorker } from "playwright-msw";
import { createWorkerFixture } from "playwright-msw";
import type { CoverageMapData } from "istanbul-lib-coverage";
import fs from "fs";
import path from "path";
import { randomUUID } from "crypto";

import { setupHandlers } from "../../src/mocks/handlers";

const test = base.extend<{
  worker: MockServiceWorker;
  http: typeof http;
}>({
  worker: createWorkerFixture(
    setupHandlers("https://mocked-out-api", "playwright"),
  ),
  http,
});

const nycOutputDir = path.join(process.cwd(), "playwright", ".nyc_output")

test.afterEach(async ({ page }) => {
  const cov = await page.evaluate(() => (globalThis as unknown as CoverageMapData | undefined)?.__coverage__);
  if (cov) {
    fs.mkdirSync(nycOutputDir, { recursive: true });
    fs.writeFileSync(
      path.join(nycOutputDir, `pw-${randomUUID()}.json`),
      JSON.stringify(cov),
    );
  }
});

export { expect, test };
