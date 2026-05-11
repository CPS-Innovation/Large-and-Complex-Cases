import * as path from "path";
import type { EgressWorkspace } from "./types";

export interface RegisterCaseSharedState {
  workspace: EgressWorkspace;
  caseUrn: string;
  // Internal case id captured from registerCase. Not currently consumed by
  // the fixtures but persisted so the setup project's state doubles as a
  // complete "connected case" snapshot — e.g. for populating DEFAULT_CASE_ID
  // after a one-off setup run.
  caseId?: number;
}

// State files written by the register-case-setup project and read by every
// register-case test fixture. Kept outside the tests/ directory so they can
// be imported from specs without Playwright treating the import as a
// cross-test-file dependency.
const root = path.resolve(__dirname, "..");
export const AUTH_FILE = path.join(root, ".auth", "register-case.json");
export const STATE_FILE = path.join(root, ".state", "register-case.json");
