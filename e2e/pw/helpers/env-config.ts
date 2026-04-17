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
    egressTemplateId: optionalEnv(
      "EGRESS_TEMPLATE_ID",
      "59a6855307087630eb190282"
    ),
    egressAdminRoleId: optionalEnv(
      "EGRESS_ADMIN_ROLE_ID",
      "591dab08368b665c9c5c5fe0"
    ),

    testFileSizeMb: parseInt(optionalEnv("TEST_FILE_SIZE_MB", "100"), 10),
    testFileCount: parseInt(optionalEnv("TEST_FILE_COUNT", "1"), 10),

    defaultWorkspaceId: process.env.DEFAULT_WORKSPACE_ID || "",
    defaultWorkspaceName: process.env.DEFAULT_WORKSPACE_NAME || "",
    defaultCaseUrn: process.env.DEFAULT_CASE_URN || "",
    defaultCaseId: process.env.DEFAULT_CASE_ID || "",
  };
}

export type EnvConfig = ReturnType<typeof loadEnvConfig>;
