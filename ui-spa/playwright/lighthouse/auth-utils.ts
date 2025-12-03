import { expect } from '@playwright/test';
import type { Page, BrowserContext } from 'playwright';
import fs from 'fs-extra';
import * as path from 'path';
import { Config } from './config.ts';
import { fileURLToPath } from 'url'

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * Authentication utilities for Lighthouse and Playwright tests
 * Handles both Azure AD authentication and CMS authentication with cookies
 */

export interface AuthConfig {
  azureAd: {
    tenantId: string;
    clientId: string;
    clientSecret: string;
    scope: string;
  };
  cms: {
    baseUrl: string;
    username: string;
    password: string;
    accessKey: string;
  };
}

export interface AuthTokens {
  azureAdToken: string;
  cmsAuthCookie: string;
  expiryTime: Date;
}

export class AuthenticationManager {
  private config: AuthConfig;
  private authFile: string;

  constructor(config: AuthConfig) {
    this.config = config;
    this.authFile = path.join(__dirname, '../test-results/auth-tokens.json');
  }

  /**
   * Authenticate using Azure AD and CMS authentication
   * Returns tokens that can be used for API calls and browser context
   */
  async authenticate(): Promise<AuthTokens> {
    console.log('=== Starting Authentication Process ===');

    // Try to load cached tokens first
   // const cachedTokens = await this.loadCachedTokens();
    // if (cachedTokens && !this.isTokenExpired(cachedTokens)) {
    //   console.log('✓ Using cached authentication tokens');
    //   console.log(`   Cache expires: ${cachedTokens.expiryTime}`);
    //   return cachedTokens;
    // }

    // if (cachedTokens) {
    //   console.log(':warning:  Cached tokens expired, refreshing...');
    // }

    // Perform fresh authentication
    console.log('→ Step 1/2: Authenticating with Azure AD...');
    const azureAdToken = await this.authenticateAzureAd();

    console.log('→ Step 2/2: Authenticating with CMS...');
    const cmsAuthCookie = await this.authenticateCMS(azureAdToken);

    const tokens: AuthTokens = {
      azureAdToken,
      cmsAuthCookie,
      expiryTime: new Date(Date.now() + 55 * 60 * 1000) // 55 minutes expiry
    };

    // Cache the tokens
    //await this.cacheTokens(tokens);

    console.log('✓ Authentication completed successfully');
    console.log(`   Tokens cached for 55 minutes`);
    return tokens;
  }

  /**
   * Authenticate using Azure AD client credentials flow
   * 
   * Note: This returns an app token (not a user token), but that's fine because:
   * 1. We inject fake MSAL tokens into the browser so the React app thinks user is logged in
   * 2. We use request interception to inject the app token into API calls
   * 3. The backend accepts app tokens for API authentication
   */
    private async authenticateAzureAd(): Promise<string> {
    console.log('→ Authenticating with Azure AD (client credentials)...');

    const { tenantId, clientId, clientSecret, scope } = this.config.azureAd;

    console.log(`   Token URL: https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`);
    console.log(`   Scope: ${scope}`);

    if (!tenantId || !clientId || !clientSecret || !scope) {
      console.error('   ✗ Missing config - tenantId:', !!tenantId, 'clientId:', !!clientId, 'clientSecret:', !!clientSecret, 'scope:', !!scope);
      throw new Error('Missing required Azure AD configuration');
    }

    const tokenUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;

    const form = new URLSearchParams({
      client_id: clientId,
      client_secret: clientSecret,
      scope: scope,
      grant_type: 'client_credentials'
    });

    const response = await fetch(tokenUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: form
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error(`   ✗ Azure AD error response: ${errorText}`);
      throw new Error(`Azure AD authentication failed: ${response.status} ${response.statusText}`);
    }

    const data = await response.json();

    if (!data.access_token) {
      throw new Error('No access token received from Azure AD');
    }

    console.log('✓ Azure AD authentication successful (app token for API calls)');
    return data.access_token;
  }

  /**
   * Authenticate with CMS 
   * Note: Azure AD token is NOT sent to /authenticate endpoint (only x-functions-key)
   */
   private async authenticateCMS(azureAdToken: string): Promise<string> {
    console.log('→ Authenticating with CMS...');

    const { baseUrl, accessKey, username, password } = this.config.cms;

    if (!baseUrl) {
      throw new Error('CMS_BASE_URL is not set! This should be your backend API URL (e.g., https://your-backend.com)');
    }

    const authUrl = `${baseUrl}/authenticate`;
    console.log(`   CMS Auth URL: ${authUrl}`);
    console.log(`   Username: ${username}`);
    console.log(`   Access Key: ${accessKey ? '✓ set' : '✗ missing'}`);

    try {
      // /authenticate only needs x-functions-key, NOT Bearer token
      const response = await fetch(authUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          'x-functions-key': accessKey
        },
        body: new URLSearchParams({
          username: username,
          password: password
        })
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error(`   ✗ CMS error response (${response.status}): ${errorText}`);
        throw new Error(`CMS authentication failed: ${response.status} ${response.statusText} - ${errorText}`);
      }

      const data = await response.json();

      // URL-encode the authentication response for cookie usage
      const compactJson = JSON.stringify(data);
      const cmsAuthCookie = encodeURIComponent(compactJson);

      console.log('✓ CMS authentication successful');
      console.log(`   Response keys: ${Object.keys(data).join(', ')}`);
      console.log(`   Cookie length: ${cmsAuthCookie.length} characters`);
      return cmsAuthCookie;
    } catch (error: any) {
      if (error.message.includes('fetch failed') || error.message.includes('ENOTFOUND')) {
        throw new Error(
          `Cannot reach CMS authentication endpoint at ${authUrl}.\n` +
          `   Check CMS_BASE_URL in .env is correct and the backend is running.`
        );
      }
      throw error;
    }
  }

      /**
   * Set authentication headers and cookies on browser context
   */
  async setAuthContext(context: BrowserContext, tokens: AuthTokens): Promise<void> {
    console.log('\n:closed_lock_with_key: Setting authentication context...');

    // Set default headers for all requests (for API calls)
    const headers = {
      'Authorization': `Bearer ${tokens.azureAdToken}`,
    };
    console.log('   Setting Authorization header: Bearer', tokens.azureAdToken.substring(0, 20) + '...');
    await context.setExtraHTTPHeaders(headers);

    // Set CMS authentication cookie
    const domain = new URL(Config.baseUrl).hostname;
    console.log(`   Setting Cms-Auth-Values cookie on domain: ${domain}`);

    const cookies = [
      {
        name: 'Cms-Auth-Values',
        value: tokens.cmsAuthCookie,
        domain: domain,
        path: '/',
        httpOnly: false,
        secure: Config.baseUrl.startsWith('https://')
      }
    ];

    await context.addCookies(cookies);

    // Verify cookies were set
    const setCookies = await context.cookies();
    console.log('   Cookies set:', setCookies.map(c => c.name).join(', '));

    console.log('✓ Authentication context set on browser');
  }

  /**
   * Setup request interception to inject Authorization header into all API requests.
   * This allows real API calls with authentication while bypassing MS login with mock auth.
   * 
   * For hybrid testing (MOCK_AUTH=true):
   * - Frontend thinks user is logged in via mock
   * - Backend receives real auth tokens for API requests
   */
  async setupRequestInterception(page: Page, tokens: AuthTokens): Promise<void> {
    console.log(':closed_lock_with_key: Setting up request interception for API auth...');

    // Intercept all requests and inject auth headers into API calls only
    await page.route('**/*', async (route) => {
      const request = route.request();
      const url = request.url();

      // Only inject auth into backend API requests
      if (url.includes('/v1/') || url.includes('/api/')) {
        const existingHeaders = request.headers();

        // Build the Cookie header by appending our auth cookie to any existing cookies
        const existingCookies = existingHeaders['cookie'] || '';
        const newCookie = `Cms-Auth-Values=${tokens.cmsAuthCookie}`;
        const cookieHeader = existingCookies
          ? `${existingCookies}; ${newCookie}`
          : newCookie;

        const headers = {
          ...existingHeaders,
          'Authorization': `Bearer ${tokens.azureAdToken}`,
          'cookie': cookieHeader,
        };

        console.log(`   :outbox_tray: Injecting auth into: ${request.method()} ${url.substring(url.indexOf('/api/') || url.indexOf('/v1/'))}`);
        await route.continue({ headers });
      } else {
        // Let all other requests through without modification
        await route.continue();
      }
    });

    console.log('✓ Request interception configured');
    console.log('✓ Real auth tokens will be injected into all backend API calls');
  }

  /**
   * Inject MSAL tokens into browser sessionStorage so the React app thinks user is logged in.
   * This must be called after navigating to the app with ?automation-test-first-visit param.
   * 
   * Note: 
   * - MSAL uses sessionStorage by default (not localStorage)
   * - For client credentials flow, we create synthetic user data since app tokens
   *   don't contain user claims
   * - The React app only checks for MSAL account existence, not the actual claims
   * - Actual API authentication uses the real app token via request interception
   */
  async injectMsalTokens(page: Page, tokens: AuthTokens): Promise<void> {
    console.log('\n:closed_lock_with_key: Injecting MSAL tokens into browser storage...');

    const clientId = process.env.AZURE_AD_CLIENT_ID || '';
    const tenantId = process.env.AZURE_AD_TENANT_ID || '';
    const username = process.env.CMS_USERNAME || '';

    // Decode the JWT to check what type of token we have
    const tokenParts = tokens.azureAdToken.split('.');
    const tokenPayload = JSON.parse(Buffer.from(tokenParts[1], 'base64').toString());

    // For client credentials flow, create synthetic user identity
    // The app only checks if an MSAL account exists, not the actual claims
    const syntheticUserId = tokenPayload.oid || tokenPayload.appid || 'automation-test-user';
    const syntheticUserName = tokenPayload.name || username || 'Automation Test User';

    const homeAccountId = `${syntheticUserId}.${tenantId}`;

    // IMPORTANT: The environment must match what MSAL actually uses in your app
    // Check your app's localStorage after real login to verify the correct environment
    // Common values: 'login.microsoftonline.com', 'login.windows.net'
    const environment = process.env.MSAL_ENVIRONMENT || 'login.windows.net';

    console.log(`   Creating synthetic user account for MSAL:`);
    console.log(`   - User ID: ${syntheticUserId}`);
    console.log(`   - Username: ${username}`);
    console.log(`   - Name: ${syntheticUserName}`);

    // Check if we need to inject group claims for private beta authorization
    const privateBetaGroup = process.env.VITE_PRIVATE_BETA_USER_GROUP || process.env.PRIVATE_BETA_USER_GROUP;
    const groupClaims = privateBetaGroup ? [privateBetaGroup] : [];

    if (privateBetaGroup) {
      console.log(`   - Adding private beta group: ${privateBetaGroup}`);
    }

    // IMPORTANT: Two different scopes!
    // 1. Azure AD scope (for token acquisition): api://xxx/.default
    // 2. MSAL UI scope (what browser expects): api://xxx/user_impersonation
    const msalUiScope = process.env.MSAL_UI_SCOPE || 'api://87c88580-01d4-477d-9b1d-ebb22c8d0987/user_impersonation';
    console.log(`   - MSAL UI Scope (for browser): ${msalUiScope}`);

    const accountKey = `${homeAccountId}-${environment}-${tenantId}`;
    const accountEntity = {
      homeAccountId: homeAccountId,
      environment: environment,
      tenantId: tenantId,
      username: username,
      localAccountId: syntheticUserId,
      name: syntheticUserName,
      authorityType: 'MSSTS',
      clientInfo: Buffer.from(JSON.stringify({ uid: syntheticUserId, utid: tenantId })).toString('base64'),
      // Include idTokenClaims with groups for PrivateBetaAuthorisation check
      idTokenClaims: {
        groups: groupClaims,
        oid: syntheticUserId,
        tid: tenantId,
        name: syntheticUserName,
        preferred_username: username,
        ver: '2.0'
      }
    };

    const now = Math.floor(Date.now() / 1000);

    // Create access token for User.Read (MSAL default scope)
    const userReadTokenKey = `${homeAccountId}-${environment}-accesstoken-${clientId}-${tenantId}-user.read profile openid email--`;
    const userReadTokenEntity = {
      homeAccountId: homeAccountId,
      environment: environment,
      credentialType: 'AccessToken',
      clientId: clientId,
      secret: tokens.azureAdToken,
      realm: tenantId,
      target: 'User.Read profile openid email',
      cachedAt: now.toString(),
      expiresOn: (now + 3600).toString(),
      extendedExpiresOn: (now + 7200).toString(),
    };

    // Create access token for API scope (THE IMPORTANT ONE!)
    // Use the UI scope here (user_impersonation), not the .default scope
    const apiTokenKey = `${homeAccountId}-${environment}-accesstoken-${clientId}-${tenantId}-${msalUiScope.toLowerCase()}--`;
    const apiTokenEntity = {
      homeAccountId: homeAccountId,
      environment: environment,
      credentialType: 'AccessToken',
      clientId: clientId,
      secret: tokens.azureAdToken, // This is the real client credentials token
      realm: tenantId,
      target: msalUiScope, // UI expects user_impersonation scope
      cachedAt: now.toString(),
      expiresOn: (now + 3600).toString(),
      extendedExpiresOn: (now + 7200).toString(),
    };

    // Create ID token entity  
    const idTokenKey = `${homeAccountId}-${environment}-idtoken-${clientId}-${tenantId}---`;
    const idTokenEntity = {
      homeAccountId: homeAccountId,
      environment: environment,
      credentialType: 'IdToken',
      clientId: clientId,
      secret: tokens.azureAdToken,
      realm: tenantId,
    };

    // Create refresh token entity (required by MSAL)
    const refreshTokenKey = `${homeAccountId}-${environment}-refreshtoken-${clientId}----`;
    const refreshTokenEntity = {
      homeAccountId: homeAccountId,
      environment: environment,
      credentialType: 'RefreshToken',
      clientId: clientId,
      secret: 'fake-refresh-token', // Not used since we intercept requests
    };

    // Inject into sessionStorage (MSAL default storage location)
    await page.evaluate(({ accountKey, accountEntity, userReadTokenKey, userReadTokenEntity, apiTokenKey, apiTokenEntity, idTokenKey, idTokenEntity, refreshTokenKey, refreshTokenEntity, clientId }) => {
      // Account
      sessionStorage.setItem(accountKey, JSON.stringify(accountEntity));

      // Tokens
      sessionStorage.setItem(userReadTokenKey, JSON.stringify(userReadTokenEntity));
      sessionStorage.setItem(apiTokenKey, JSON.stringify(apiTokenEntity));
      sessionStorage.setItem(idTokenKey, JSON.stringify(idTokenEntity));
      sessionStorage.setItem(refreshTokenKey, JSON.stringify(refreshTokenEntity));

      // MSAL metadata
      sessionStorage.setItem('msal.account.keys', JSON.stringify([accountKey]));
      sessionStorage.setItem(`msal.token.keys.${clientId}`, JSON.stringify([
        userReadTokenKey,
        apiTokenKey,
        idTokenKey,
        refreshTokenKey
      ]));
    }, { accountKey, accountEntity, userReadTokenKey, userReadTokenEntity, apiTokenKey, apiTokenEntity, idTokenKey, idTokenEntity, refreshTokenKey, refreshTokenEntity, clientId });

    console.log('   ✓ MSAL account entity injected');
    console.log('   ✓ MSAL access token (User.Read) injected');
    console.log('   ✓ MSAL access token (API scope) injected');
    console.log('   ✓ MSAL ID token injected');
    console.log('   ✓ MSAL refresh token injected');
    console.log('   ✓ MSAL metadata keys injected');
    console.log('✓ MSAL tokens injected into sessionStorage (6 keys total)');
    console.log('✓ React app will now think user is authenticated');

    // Debug: Verify what was actually stored
    const storedKeys = await page.evaluate(() => {
      return Object.keys(sessionStorage).filter(k => k.includes('login.') || k.includes('msal') || k.includes('windows.net'));
    });
    console.log(`   Debug: Stored MSAL keys in sessionStorage: ${storedKeys.length} keys`);
    storedKeys.forEach(key => console.log(`     - ${key}`));
  }

  /**
   * Perform interactive authentication for Playwright tests
   * This simulates the real user login flow
   */
  async interactiveAuth(page: Page): Promise<void> {
    console.log('→ Starting interactive authentication...');

    // Navigate to the application
    await page.goto(Config.baseUrl);

    // Wait for redirect to Azure AD login
    await page.waitForURL('**/login.microsoftonline.com/**', { timeout: 10000 });

    // Fill in Azure AD credentials
    await page.locator('input[type="email"]').fill(this.config.cms.username);
    await page.locator('input[type="submit"]').click();

    await page.waitForSelector('input[type="password"]', { timeout: 5000 });
    await page.locator('input[type="password"]').fill(this.config.cms.password);
    await page.locator('input[type="submit"]').click();

    // Handle "Stay signed in?" prompt if it appears
    try {
      await page.locator('input[type="submit"][value="Yes"]').click({ timeout: 3000 });
    } catch {
      // Ignore if the prompt doesn't appear
    }

    // Wait for redirect back to application
    await page.waitForURL(Config.baseUrl, { timeout: 15000 });

    // Verify successful login
    await expect(page.locator('[data-testid="user-info"]')).toBeVisible({ timeout: 10000 });

    console.log('✓ Interactive authentication completed');
  }

  /**
   * Cache authentication tokens to file
   */
  private async cacheTokens(tokens: AuthTokens): Promise<void> {
    await fs.ensureDir(path.dirname(this.authFile));
    await fs.writeJson(this.authFile, tokens, { spaces: 2 });
  }

  /**
   * Load cached authentication tokens from file
   */
  private async loadCachedTokens(): Promise<AuthTokens | null> {
    try {
      if (await fs.pathExists(this.authFile)) {
        const tokens = await fs.readJson(this.authFile);
        return tokens;
      }
    } catch (error) {
      console.warn('Failed to load cached tokens:', error);
    }
    return null;
  }

  /**
   * Check if authentication tokens are expired
   */
  private isTokenExpired(tokens: AuthTokens): boolean {
    return new Date() >= tokens.expiryTime;
  }

  /**
   * Clear cached authentication tokens
   */
  async clearCache(): Promise<void> {
    try {
      await fs.remove(this.authFile);
      console.log('✓ Authentication cache cleared');
    } catch (error) {
      console.warn('Failed to clear auth cache:', error);
    }
  }
}

/**
 * Create authentication manager with environment configuration
 */
export function createAuthManager(): AuthenticationManager {
  console.log('\n:wrench: DEBUG: Creating auth manager with config:');
  console.log('   AZURE_AD_TENANT_ID:', process.env.AZURE_AD_TENANT_ID ? '✓ set' : '✗ missing');
  console.log('   AZURE_AD_CLIENT_ID:', process.env.AZURE_AD_CLIENT_ID ? '✓ set' : '✗ missing');
  console.log('   AZURE_AD_CLIENT_SECRET:', process.env.AZURE_AD_CLIENT_SECRET ? '✓ set' : '✗ missing');
  console.log('   AZURE_AD_SCOPE:', process.env.AZURE_AD_SCOPE || 'api://user_impersonation (default)');
  console.log('   CMS_BASE_URL:', process.env.CMS_BASE_URL || Config.baseUrl + ' (from Config)');
  console.log('   CMS_USERNAME:', process.env.CMS_USERNAME ? '✓ set' : '✗ missing');
  console.log('   CMS_PASSWORD:', process.env.CMS_PASSWORD ? '✓ set' : '✗ missing');
  console.log('   CMS_ACCESS_KEY:', process.env.CMS_ACCESS_KEY ? '✓ set' : '✗ missing');

  const authConfig: AuthConfig = {
    azureAd: {
      tenantId: process.env.AZURE_AD_TENANT_ID || '',
      clientId: process.env.AZURE_AD_CLIENT_ID || '',
      clientSecret: process.env.AZURE_AD_CLIENT_SECRET || '',
      scope: process.env.AZURE_AD_SCOPE || 'api://user_impersonation'
    },
    cms: {
      baseUrl: process.env.CMS_BASE_URL || Config.baseUrl,
      username: process.env.CMS_USERNAME || '',
      password: process.env.CMS_PASSWORD || '',
      accessKey: process.env.CMS_ACCESS_KEY || ''
    }
  };

  return new AuthenticationManager(authConfig);
}

/**
 * Environment variable validation
 */
export function validateAuthEnvironment(): void {
  const requiredVars = [
    'AZURE_AD_TENANT_ID',
    'AZURE_AD_CLIENT_ID',
    'AZURE_AD_CLIENT_SECRET',
    'CMS_USERNAME',
    'CMS_PASSWORD',
    'CMS_ACCESS_KEY'
  ];

  const missing = requiredVars.filter(varName => !process.env[varName]);

  if (missing.length > 0) {
    throw new Error(
      `Missing required environment variables for authentication: ${missing.join(', ')}\n` +
      'Please set these variables in your .env file or environment.'
    );
  }
}