// Non-sensitive shared Egress identifiers used by the E2E setup helpers.
// These are environment-wide IDs exposed by the Egress admin API (the workspace
// template and the admin role assigned to the test user) — not credentials or
// tenant secrets — so they are safe to commit. They can still be overridden per
// environment via EGRESS_TEMPLATE_ID / EGRESS_ADMIN_ROLE_ID.
export const EGRESS_TEMPLATE_ID = "59a6855307087630eb190282";
export const EGRESS_ADMIN_ROLE_ID = "591dab08368b665c9c5c5fe0";
