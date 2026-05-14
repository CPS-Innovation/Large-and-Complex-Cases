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
      `Invalid ${name}: expected a positive integer, got "${raw}"`,
    );
  }
  return parsed;
}

export function loadEnvConfig() {
  return {
    baseUrl: requireEnv("BASE_URL"),
    cmsLoginPage: requireEnv("CMS_LOGIN_PAGE"),
    egressBaseUrl: requireEnv("EGRESS_BASE_URL"),
    // Required only for register-case.
    caseApiBaseUrl: process.env.CASE_API_BASE_URL || "",

    tenantId: requireEnv("TENANT_ID"),
    lccApiClientId: requireEnv("LCC_API_CLIENT_ID"),
    // CMRC Client ID, required only for register-case.
    cmrcApiClientId: process.env.CMRC_API_CLIENT_ID || "",

    e2eAdUser: requireEnv("E2E_AD_USER"),
    e2eAdPassword: requireEnv("E2E_AD_PASSWORD"),
    cmsUsername: requireEnv("CMS_USERNAME"),
    cmsPassword: requireEnv("CMS_PASSWORD"),
    // DDEI base url and function key for the register-case endpoint. 
    // The register-case path validates presence at the callsite and
    // throws a clear error if missing.
    ddeiBaseUrl: process.env.DDEI_BASE_URL || "",
    ddeiAccessKeyCaseRegister: process.env.DDEI_ACCESS_KEY_CASE_REGISTER || "",

    egressServiceAccountAuth: requireEnv("EGRESS_SERVICE_ACCOUNT_AUTH"),
    egressTemplateId: optionalEnv("EGRESS_TEMPLATE_ID", EGRESS_TEMPLATE_ID),
    egressAdminRoleId: optionalEnv(
      "EGRESS_ADMIN_ROLE_ID",
      EGRESS_ADMIN_ROLE_ID,
    ),

    testFileSizeMb: positiveIntEnv("TEST_FILE_SIZE_MB", "100"),
    testFileCount: positiveIntEnv("TEST_FILE_COUNT", "1"),

    defaultWorkspaceId: process.env.DEFAULT_WORKSPACE_ID || "",
    defaultWorkspaceName: process.env.DEFAULT_WORKSPACE_NAME || "",
    defaultCaseUrn: process.env.DEFAULT_CASE_URN || "",
    defaultCaseId: process.env.DEFAULT_CASE_ID || "",

    // LCC backend base URL — needed for the post-test NetApp file
    // teardown which posts to /api/v1/netapp/delete/batch.
    // Optional: if unset, NetApp teardown is silently skipped.
    lccApiBaseUrl: process.env.LCC_API_BASE_URL || "",
    // The connected NetApp folder used as the sourcePath prefix for the
    // batch-delete teardown (e.g. "Automation-Testing"). The backend
    // validates each sourcePath starts with the case's stored
    // NetappFolderPath, so this must match the folder the case is
    // connected to. Optional: if unset, NetApp teardown is silently skipped.
    netAppOperationName: process.env.NETAPP_OPERATION_NAME || "",
    // App registration for the LCC API (client-credentials flow). Used
    // by disassociate-NetApp-connection in register-case teardown —
    // that endpoint requires an app-only token, not user-delegated.
    // Optional: if unset, disassociation is silently skipped.
    lccApiClientSecret: process.env.LCC_API_CLIENT_SECRET || "",
  };
}

export type EnvConfig = ReturnType<typeof loadEnvConfig>;
