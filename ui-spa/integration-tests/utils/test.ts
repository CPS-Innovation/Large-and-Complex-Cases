import { test as base, expect } from "@playwright/test";
import { http } from "msw";
import type { MockServiceWorker } from "playwright-msw";
import { createWorkerFixture } from "playwright-msw";
import fs from "fs";
import path from "path";
import { randomUUID } from "crypto";

import { setupHandlers } from "../../src/mocks/handlers";

const istanbulCLIOutput = path.join(process.cwd(), "coverage", ".nyc_output");

const test = base.extend<{
  worker: MockServiceWorker;
  http: typeof http;
}>({
  worker: createWorkerFixture(
    setupHandlers("https://mocked-out-api", "playwright"),
  ),
  http,
  // collect coverage:
  context: async ({ context }, use) => {
    await context.addInitScript(() =>
      window.addEventListener("beforeunload", () =>
        (window as any).collectIstanbulCoverage(
          JSON.stringify((window as any).__coverage__),
        ),
      ),
    );
    await fs.promises.mkdir(istanbulCLIOutput, { recursive: true });
    await context.exposeFunction(
      "collectIstanbulCoverage",
      (coverageJSON: string) => {
        if (coverageJSON)
          fs.writeFileSync(
            path.join(
              istanbulCLIOutput,
              `playwright_coverage_${randomUUID()}.json`,
            ),
            coverageJSON,
          );
      },
    );
    await use(context);
    for (const page of context.pages()) {
      await page.evaluate(() =>
        (window as any).collectIstanbulCoverage(
          JSON.stringify((window as any).__coverage__),
        ),
      );
    }
  },
});

export { expect, test };
