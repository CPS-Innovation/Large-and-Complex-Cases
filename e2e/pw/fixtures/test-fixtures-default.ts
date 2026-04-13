import { test as base, expect } from "@playwright/test";
import { setupDefaultTestData } from "./setup-helper-default";
import type { TestSetupResult } from "../helpers/types";

export const test = base.extend<{ testData: TestSetupResult }>({
  // Default mode: uploads to known workspace, uses existing case (already connected)
  // Skips workspace creation, case registration, Egress/NetApp connect
  testData: async ({ page }, use) => {
    const result = await setupDefaultTestData(page);
    await use(result);
  },
});

export { expect };
