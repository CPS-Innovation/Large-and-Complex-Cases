import { test as base, expect } from "@playwright/test";
import { setupTestData } from "./setup-helper";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Test-scoped: each test gets its own workspace, case, and browser login
  // Uses default file size/count from env vars (TEST_FILE_SIZE_MB, TEST_FILE_COUNT)
  testData: async ({ page }, use) => {
    const result = await setupTestData(page);
    await use(result);
  },
});

export { expect };
