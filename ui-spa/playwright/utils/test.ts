import { test as base, expect } from "@playwright/test";
import { http } from "msw";
import type { MockServiceWorker } from "playwright-msw";
import { createWorkerFixture } from "playwright-msw";

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

export { expect, test };
