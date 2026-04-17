import { test as base, expect } from "@playwright/test";
import { setupDefaultTestData } from "./setup-helper-default";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Default mode with large file: 1 x 200MB, uses existing case
  testData: [async ({ page }, use) => {
    const result = await setupDefaultTestData(page, {
      fileSizeMb: 200,
      fileCount: 1,
    });
    await use(result);
  }, { timeout: 300_000 }],
});

export { expect };
