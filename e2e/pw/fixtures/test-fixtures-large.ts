import { test as base, expect } from "@playwright/test";
import { setupTestData } from "./setup-helper";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Large-file variant: 1 x 200MB file
  testData: [async ({ page }, use) => {
    const result = await setupTestData(page, {
      fileSizeMb: 200,
      fileCount: 1,
    });
    await use(result);
  }, { timeout: 300_000 }],
});

export { expect };
