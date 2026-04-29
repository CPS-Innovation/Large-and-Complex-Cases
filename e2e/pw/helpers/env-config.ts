import { EGRESS_TEMPLATE_ID, EGRESS_ADMIN_ROLE_ID } from "./constants";

function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

function optionalEnv(name: string, defaultValue: string): string {
  return process.env[name] || defaultValue;
}

function positiveIntEnv(name: string, defaultValue: string): number {
  const raw = optionalEnv(name, defaultValue);
  const parsed = Number.parseInt(raw, 10);
  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error(
      `Invalid ${name}: expected a positive integer, got "${raw}"`
    );
  }
  return parsed;
}

export function loadEnvConfig() {
  return {
    baseUrl: requireEnv("BASE_URL"),
    cmsLoginPage: requireEnv("CMS_LOGIN_PAGE"),
    caseApiBaseUrl: requireEnv("CASE_API_BASE_URL"),
    ddeiBaseUrl: requireEnv("DDEI_BASE_URL"),
    egressBaseUrl: requireEnv("EGRESS_BASE_URL"),

    tenantId: requireEnv("TENANT_ID"),
    clientId: requireEnv("CLIENT_ID"),

    e2eAdUser: requireEnv("E2E_AD_USER"),
    e2eAdPassword: requireEnv("E2E_AD_PASSWORD"),
    cmsUsername: requireEnv("CMS_USERNAME"),
    cmsPassword: requireEnv("CMS_PASSWORD"),
    ddeiAccessKey: requireEnv("DDEI_ACCESS_KEY"),

    egressServiceAccountAuth: requireEnv("EGRESS_SERVICE_ACCOUNT_AUTH"),
    egressTemplateId: optionalEnv("EGRESS_TEMPLATE_ID", EGRESS_TEMPLATE_ID),
    egressAdminRoleId: optionalEnv("EGRESS_ADMIN_ROLE_ID", EGRESS_ADMIN_ROLE_ID),

    testFileSizeMb: positiveIntEnv("TEST_FILE_SIZE_MB", "100"),
    testFileCount: positiveIntEnv("TEST_FILE_COUNT", "1"),

    defaultWorkspaceId: process.env.DEFAULT_WORKSPACE_ID || "",
    defaultWorkspaceName: process.env.DEFAULT_WORKSPACE_NAME || "",
    defaultCaseUrn: process.env.DEFAULT_CASE_URN || "",
    defaultCaseId: process.env.DEFAULT_CASE_ID || "",

    // LCC backend base URL — needed for the post-test NetApp file
    // teardown which hits DELETE /api/v1/cases/{operationName}/netapp.
    // Optional: if unset, NetApp teardown is silently skipped.
    lccApiBaseUrl: process.env.LCC_API_BASE_URL || "",
    // The connected NetApp folder name used as the {operationName} path
    // segment (and S3 prefix) for NetApp deletions, e.g. "Automation-Testing".
    netAppOperationName: process.env.NETAPP_OPERATION_NAME || "",
  };
}

export type EnvConfig = ReturnType<typeof loadEnvConfig>;
