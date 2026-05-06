import type { AuthTokens } from "./types";

/**
 * Mint an Azure AD app-only token via the client-credentials grant.
 * Used for endpoints that require app-roles rather than user-delegated
 * scopes (e.g. DELETE /api/v1/netapp/connections). The resulting token
 * has the LCC API as both client and audience, with `.default` scope
 * which surfaces app role assignments rather than per-scope consents.
 */
export async function getAzureADAppToken(
  tenantId: string,
  clientId: string,
  clientSecret: string
): Promise<string> {
  const tokenUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;

  const body = new URLSearchParams({
    client_id: clientId,
    client_secret: clientSecret,
    scope: `api://${clientId}/.default`,
    grant_type: "client_credentials",
  });

  const response = await fetch(tokenUrl, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: body.toString(),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `Azure AD app-token request failed (${response.status}): ${text}`
    );
  }

  const data = await response.json();
  return data.access_token;
}

export async function getAzureADToken(
  tenantId: string,
  clientId: string,
  username: string,
  password: string
): Promise<string> {
  const tokenUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;

  const body = new URLSearchParams({
    client_id: clientId,
    scope: `api://${clientId}/user_impersonation`,
    username,
    password,
    grant_type: "password",
  });

  const response = await fetch(tokenUrl, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: body.toString(),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `Azure AD token request failed (${response.status}): ${text}`
    );
  }

  const data = await response.json();
  return data.access_token;
}

export async function getCmsAuth(
  ddeiBaseUrl: string,
  ddeiAccessKey: string,
  accessToken: string,
  username: string,
  password: string
): Promise<string> {
  const body = new URLSearchParams({ username, password });

  const response = await fetch(`${ddeiBaseUrl}/authenticate`, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      "x-functions-key": ddeiAccessKey,
      Authorization: `Bearer ${accessToken}`,
    },
    body: body.toString(),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`CMS auth request failed (${response.status}): ${text}`);
  }

  const data = await response.json();
  return encodeURIComponent(JSON.stringify(data));
}

export async function getAuthTokens(
  tenantId: string,
  clientId: string,
  adUsername: string,
  adPassword: string,
  ddeiBaseUrl: string,
  ddeiAccessKey: string,
  cmsUsername: string,
  cmsPassword: string
): Promise<AuthTokens> {
  // AD token must be obtained first — DDEI requires it
  const accessToken = await getAzureADToken(
    tenantId,
    clientId,
    adUsername,
    adPassword
  );

  const cmsAuth = await getCmsAuth(
    ddeiBaseUrl,
    ddeiAccessKey,
    accessToken,
    cmsUsername,
    cmsPassword
  );

  return { accessToken, cmsAuth };
}
