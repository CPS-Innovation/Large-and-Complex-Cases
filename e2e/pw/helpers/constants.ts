// Non-sensitive shared Egress identifiers used by the E2E setup helpers.
// These are environment-wide IDs exposed by the Egress admin API (the workspace
// template and the admin role assigned to the test user) — not credentials or
// tenant secrets — so they are safe to commit. They can still be overridden per
// environment via EGRESS_TEMPLATE_ID / EGRESS_ADMIN_ROLE_ID.
export const EGRESS_TEMPLATE_ID = "59a6855307087630eb190282";
export const EGRESS_ADMIN_ROLE_ID = "591dab08368b665c9c5c5fe0";

// Canonical NetApp source fixture used by every NetApp -> Egress spec.
// Must be present at `<folder>/<NETAPP_FIXTURE_FILENAME>` on the shared
// drive before tests run; seed once per mode via
// `scripts/seed-netapp-fixture.ts`. The cleanup helpers
// (deleteNetAppFile, 24h workspace sweep) only target `generated-100MB-*`
// test artefacts, so this fixture is safe from automated removal.
export const NETAPP_FIXTURE_FILENAME = "lcc-e2e-fixture-source.txt";

// NetApp folder used by register-case mode. The setup project's NetApp
// connect flow attaches whatever folder is at index 0 in the connect
// list — by environment convention that's `Automation-Testing`.
// Default-mode tests connect to `existingCaseAutomation` instead and
// read NETAPP_OPERATION_NAME from env; the two modes don't share a
// NetApp folder.
export const REGISTER_CASE_NETAPP_FOLDER = "Automation-Testing";
