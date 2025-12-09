import { chromium } from 'playwright';
import type { BrowserContext } from 'playwright';
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

type StorageState = Awaited<ReturnType<BrowserContext['storageState']>>;

type UiAuthOptions = {
  baseUrl: string;
  username: string;
  password: string;
  headless?: boolean;
  screenSize?: { width: number; height: number };
  userAgent?: string;
  timeouts?: {
    loginUrl?: number;        // default 15000
    passwordField?: number;   // default 10000
    redirectBack?: number;    // default 20000
    userInfoVisible?: number; // default 10000
  };
};

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
    const cachedTokens = await this.loadCachedTokens();
    if (cachedTokens && !this.isTokenExpired(cachedTokens)) {
      console.log('✓ Using cached authentication tokens');
      return cachedTokens;
    }

    // Perform fresh authentication
    const azureAdToken = await this.authenticateAzureAd();
    const cmsAuthCookie = await this.authenticateCMS(azureAdToken);
    
    const tokens: AuthTokens = {
      azureAdToken,
      cmsAuthCookie,
      expiryTime: new Date(Date.now() + 55 * 60 * 1000) // 55 minutes expiry
    };

    // Cache the tokens
    await this.cacheTokens(tokens);
    
    console.log('✓ Authentication completed successfully');
    return tokens;
  }

  /**
   * Authenticate using Azure AD client credentials flow
   */
  private async authenticateAzureAd(): Promise<string> {
    console.log('→ Authenticating with Azure AD...');

    const { tenantId, clientId, clientSecret, scope } = this.config.azureAd;
    
    if (!tenantId || !clientId || !clientSecret || !scope) {
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
      throw new Error(`Azure AD authentication failed: ${response.status} ${response.statusText}`);
    }

    const data = await response.json();
    
    if (!data.access_token) {
      throw new Error('No access token received from Azure AD');
    }

    console.log('✓ Azure AD authentication successful');
    return data.access_token;
  }

  /**
   * Authenticate with CMS using Azure AD token
   */
  private async authenticateCMS(azureAdToken: string): Promise<string> {
    console.log('→ Authenticating with CMS...');

    const { baseUrl, accessKey } = this.config.cms;
    
    const authUrl = `${baseUrl}/authenticate`;
    
    const response = await fetch(authUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Authorization': `Bearer ${azureAdToken}`,
        'x-functions-key': accessKey
      },
      body: new URLSearchParams({
        username: this.config.cms.username,
        password: this.config.cms.password
      })
    });

    if (!response.ok) {
      throw new Error(`CMS authentication failed: ${response.statusText}`);
    }

    const data = await response.json();
    
    // URL-encode the authentication response for cookie usage
    const cmsAuthCookie = encodeURIComponent(JSON.stringify(data));
    
    console.log('✓ CMS authentication successful');
    return cmsAuthCookie;
  }

  /**
   * Set authentication headers and cookies on browser context
   */
  async setAuthContext(context: BrowserContext, tokens: AuthTokens): Promise<void> {

    const cookieName = 'Cms-Auth-Values';
    
    // Set default headers for all requests
    await context.setExtraHTTPHeaders({
      'Authorization': `Bearer ${tokens.azureAdToken}`,
      'Cookie': `${cookieName}=${tokens.cmsAuthCookie}`
    });

    // Set CMS authentication cookie
    const cookies = [
      {
        name: cookieName,
        value: tokens.cmsAuthCookie,
        domain: new URL(Config.baseUrl).hostname,
        path: '/',
        httpOnly: true,
        secure: Config.baseUrl.startsWith('https://')
      }
    ];

    await context.addCookies(cookies);
    console.log('✓ Authentication context set on browser');
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
    return Date.now() >= new Date(tokens.expiryTime).getTime();
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
  const authConfig: AuthConfig = {
    azureAd: {
      tenantId: process.env.AZURE_AD_TENANT_ID || '',
      clientId: process.env.AZURE_AD_CLIENT_ID || '',
      clientSecret: process.env.AZURE_AD_CLIENT_SECRET || '',
      scope: process.env.AZURE_AD_SCOPE || 'api://user_impersonation'
    },
    cms: {
      baseUrl: process.env.CMS_BASE_URL || '',
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

/**
 * Perform interactive authentication to Azure AD for Playwright tests
 * This simulates the real user login flow
 */
export async function interactiveAdAuth(options: UiAuthOptions): Promise<StorageState> {
  const {
    baseUrl,
    username,
    password,
    headless = true,
    screenSize,
    userAgent,
    timeouts = {},
  } = options;

  const {
    loginUrl = 15000,
    passwordField = 10000,
    redirectBack = 20000,
    userInfoVisible = 10000,
  } = timeouts;

  console.log('→ Starting interactive authentication to Azure AD...');

  // 1) Launch a temporary browser & context
  const browser = await chromium.launch({ headless });
  const context = await browser.newContext({ viewport: screenSize, userAgent });
  const page = await context.newPage();

  try {
    // 2) Navigate to your app → AAD login
    await page.goto(baseUrl);
    await page.waitForURL('**/login.microsoftonline.com/**', { timeout: loginUrl });

    // 3) Username + Next
    await page.locator('input[type="email"]').fill(username);
    await page.locator('input[type="submit"]').click();

    // 4) Password + Sign in
    await page.waitForSelector('input[type="password"]', { timeout: passwordField });
    await page.locator('input[type="password"]').fill(password);
    await page.locator('input[type="submit"]').click();

    // 5) Optional: “Stay signed in?”
    try {
      await page.locator('input[type="submit"][value="Yes"]').click({ timeout: 3000 });
    } catch {
      // ignore if not shown
    }

    // 6) Wait for redirect back to your app
    await page.waitForURL(baseUrl, { timeout: redirectBack });

    await page.pause();

    // 7) Verify login 
    const userInfo = page.locator('[data-testid="div-ad-username"]');
    const visible = await userInfo.isVisible({ timeout: userInfoVisible });
    if (!visible) throw new Error('User info not visible after login');

    // 8) Capture storageState (cookies + localStorage)
    const storageState = await context.storageState();
    console.log('✓ Interactive authentication completed');
    return storageState;
  } finally {
    await context.close();
    await browser.close();
  }
}
