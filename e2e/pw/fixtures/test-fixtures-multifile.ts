import { test as base, expect } from "@playwright/test";
import { setupTestData } from "./setup-helper";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Multi-file variant: 3 x 50MB files
  testData: [async ({ page }, use) => {
    const result = await setupTestData(page, {
      fileSizeMb: 50,
      fileCount: 3,
    });
    await use(result);
  }, { timeout: 300_000 }],
});

export { expect };
