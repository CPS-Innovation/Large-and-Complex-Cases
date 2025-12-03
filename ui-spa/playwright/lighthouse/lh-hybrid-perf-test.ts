/**
 * Hybrid Lighthouse Test: Mock Auth + Real Backend APIs
 * 
 * This approach:
 * - Uses MOCK_AUTH to bypass Microsoft login (simple!)
 * - Uses REAL backend APIs (not mocked)
 * - Injects real auth tokens into API requests
 * 
 * Best of both worlds!
 */

import { chromium } from 'playwright';
import type { Browser, BrowserContext, Page } from 'playwright';
import { Config, validateConfig } from './config.ts';
import { createAuthManager, validateAuthEnvironment } from './auth-utils.ts';
import chalk from 'chalk';

class HybridLighthouseTest {
  private browser: Browser | null = null;
  private context: BrowserContext | null = null;
  private page: Page | null = null;

  async run(): Promise<void> {
    console.log(chalk.blue(':rocket: Hybrid Lighthouse Test: Mock Auth + Real APIs\n'));

    try {
      // Validate config first
      validateAuthEnvironment();
      validateConfig();

      // Check if mock auth is enabled
      if (!Config.useMockAuth) {
        console.log(chalk.yellow(':warning:  VITE_MOCK_AUTH is not set to "true"'));
        console.log(chalk.yellow('   This test requires VITE_MOCK_AUTH=true in your .env file'));
        throw new Error('VITE_MOCK_AUTH must be set to "true" for hybrid test');
      }

      console.log(chalk.green('✓ VITE_MOCK_AUTH is enabled - MS login will be bypassed\n'));

      // Get real authentication tokens for API calls
      console.log('→ Authenticating for API access...');
      console.log('   (This may take 10-30 seconds on first run)');
      const authManager = createAuthManager();

      let tokens;
      try {
        // Add timeout to authentication
          console.log('→ TEST...');
        const authPromise = authManager.authenticate();
        const timeoutPromise = new Promise((_, reject) =>
          setTimeout(() => reject(new Error('Authentication timeout after 60 seconds')), 60000)
        );

        tokens = await Promise.race([authPromise, timeoutPromise]);
        console.log('✓ Got real authentication tokens for API calls\n');
      } catch (authError: any) {
        console.error(chalk.red(':x: Authentication failed:'), authError.message);
        console.error(chalk.yellow('\nPossible issues:'));
        console.error('  1. Azure AD credentials are incorrect or expired');
        console.error('  2. CMS authentication endpoint is unreachable');
        console.error('  3. Network connectivity issues');
        console.error('  4. Timeout - authentication took too long');
        console.error('\nCheck your .env file has:');
        console.error('  - AZURE_AD_TENANT_ID');
        console.error('  - AZURE_AD_CLIENT_ID');
        console.error('  - AZURE_AD_CLIENT_SECRET');
        console.error('  - AZURE_AD_SCOPE (e.g., api://xxx/.default)');
        console.error('  - CMS_USERNAME');
        console.error('  - CMS_PASSWORD');
        console.error('  - CMS_ACCESS_KEY');
        console.error('  - CMS_BASE_URL\n');
        throw authError;
      }

      // Initialize browser
      await this.initializeBrowser();

      if (!this.page || !this.context) throw new Error('Browser not initialized');

      // IMPORTANT: Set cookies on browser context BEFORE navigation
      console.log('→ Setting authentication cookies on browser context...');
      const domain = new URL(Config.baseUrl).hostname;
      console.log(`   Cookie domain: ${domain}`);

      await this.context.addCookies([
        {
          name: 'Cms-Auth-Values',
          value: tokens.cmsAuthCookie,
          domain: domain,
          path: '/api/',
          httpOnly: false,
          secure: Config.baseUrl.startsWith('https://')
        }
      ]);

      // Verify cookies were set
      const setCookies = await this.context.cookies();
      console.log(`   ✓ Cookies set: ${setCookies.map(c => c.name).join(', ')}`);
      console.log('');

      // Set up request interception to inject auth tokens into API calls
      console.log('→ Setting up API request interception...');
      await authManager.setupRequestInterception(this.page, tokens);
      console.log('✓ Real auth tokens will be injected into API calls\n');

      // Navigate directly to the page (mock auth will handle login bypass)
      const testUrl = `${Config.baseUrl}/case/${Config.cases[0]}/case-management/transfer-materials`;
      console.log(`→ Navigating to: ${testUrl}`);
      console.log('  (MOCK_AUTH enabled - no MS login redirect)');

      await this.page.goto(testUrl, { waitUntil: 'networkidle', timeout: 30000 });

      const finalUrl = this.page.url();
      console.log(`✓ Page loaded: ${finalUrl}\n`);

      if (finalUrl.includes('login.microsoftonline.com') || finalUrl.includes('login.windows.net')) {
        throw new Error('Redirected to MS login - check VITE_MOCK_AUTH=true is set in .env');
      }

      // Wait for data to load
      console.log('→ Waiting for transfer materials data...');
      await this.page.waitForSelector('[data-testid="tab-content-transfer-materials"]', {
        timeout: 10000
      });
      console.log('✓ Transfer materials tab loaded\n');

      // Wait for actual API data (not just empty tables)
      console.log('→ Waiting for API data to populate tables...');

      try {
        await this.page.waitForSelector('[data-testid="egress-table-wrapper"] table tbody tr', {
          timeout: 15000
        });
        console.log('✓ Egress data loaded from API');
      } catch {
        console.log(chalk.yellow(':warning:  No Egress data (table is empty)'));
      }

      try {
        await this.page.waitForSelector('[data-testid="netapp-table-wrapper"] table tbody tr', {
          timeout: 15000
        });
        console.log('✓ NetApp data loaded from API');
      } catch {
        console.log(chalk.yellow(':warning:  No NetApp data (table is empty)'));
      }

      // Verify data sections are present
      const hasEgress = await this.page.locator('[data-testid="egress-table-wrapper"]').isVisible();
      const hasNetApp = await this.page.locator('[data-testid="netapp-table-wrapper"]').isVisible();
      const egressCount = await this.page.locator('[data-testid="egress-table-wrapper"] table tbody tr').count();
      const netAppCount = await this.page.locator('[data-testid="netapp-table-wrapper"] table tbody tr').count();

      console.log(chalk.green('\n- SUCCESS! Hybrid test completed'));
      console.log(`   Egress section: ${hasEgress ? '✓ visible' : '✗ missing'} (${egressCount} items)`);
      console.log(`   NetApp section: ${hasNetApp ? '✓ visible' : '✗ missing'} (${netAppCount} items)`);
      console.log(`   Mock auth: ✓ bypassed MS login`);
      console.log(`   Real APIs: ✓ used real backend with auth tokens`);

      // Keep browser open to see the result
      if (!Config.headless) {
        console.log('\n:double_vertical_bar:  Browser will stay open. Close it when done.');
        await new Promise(() => {}); // Keep alive
      }

    } catch (error) {
      console.error(chalk.red('\n:x: Test failed:'), error);

      if (this.page) {
        await this.page.screenshot({
          path: `hybrid-test-error.png`,
          fullPage: true
        });
        console.log('Screenshot saved: hybrid-test-error.png');
      }

      // Keep browser open on failure if not headless
      if (!Config.headless) {
        console.log(chalk.yellow('\n:double_vertical_bar:  Test failed but browser will stay open for inspection.'));
        console.log(chalk.gray('   Close the browser window or press Ctrl+C when done.\n'));
        await new Promise(() => {}); // Keep alive indefinitely
      }

      throw error;
    } finally {
      if (Config.headless) {
        await this.cleanup();
      }
    }
  }

  private async initializeBrowser(): Promise<void> {
    console.log('→ Launching browser...');

    this.browser = await chromium.launch({
      headless: Config.headless,
      slowMo: Config.headless ? 0 : 100,
    });

    this.context = await this.browser.newContext({
      viewport: Config.screenSize,
    });

    this.page = await this.context.newPage();

    // Log API requests and responses for debugging
    this.page.on('request', request => {
      if (request.url().includes('/api/') || request.url().includes('/v1/')) {
        const headers = request.headers();
        const url = request.url();
        const shortUrl = url.substring(url.indexOf('/api/') !== -1 ? url.indexOf('/api/') : url.indexOf('/v1/'));
        console.log(`   :outbox_tray: ${request.method()} ${shortUrl}`);
        console.log(`      Auth: ${headers['authorization'] ? '✓ Bearer token' : '✗ MISSING'}`);
        console.log(`      Cookie: ${headers['cookie']?.includes('Cms-Auth-Values') ? '✓ CMS auth' : '✗ MISSING'}`);
      }
    });

    this.page.on('response', async response => {
      if (response.url().includes('/api/') || response.url().includes('/v1/')) {
        const status = response.status();
        const url = response.url();
        const shortUrl = url.substring(url.indexOf('/api/') !== -1 ? url.indexOf('/api/') : url.indexOf('/v1/'));

        if (status >= 200 && status < 300) {
          console.log(chalk.green(`   :inbox_tray: ${status} ${shortUrl}`));
        } else if (status >= 400) {
          console.log(chalk.red(`   :inbox_tray: ${status} ${shortUrl}`));
          try {
            const body = await response.text();
            console.log(chalk.red(`      Error: ${body.substring(0, 100)}`));
          } catch {
            // Ignore if can't read body
          }
        }
      }
    });

    console.log('✓ Browser initialized\n');
  }

  private async cleanup(): Promise<void> {
    if (this.page) await this.page.close();
    if (this.context) await this.context.close();
    if (this.browser) await this.browser.close();
  }
}

// Entry point - run the test if this file is executed directly
console.log(':mag: DEBUG: Checking if this is the main module...');
console.log('   import.meta.url:', import.meta.url);
console.log('   process.argv[1]:', process.argv[1]);

const isMainModule = import.meta.url === `file://${process.argv[1]}` ||
                     import.meta.url.endsWith(process.argv[1]?.replace(/\\/g, '/') || '');

console.log('   Is main module?', isMainModule);

if (isMainModule) {
  console.log('✓ Running as main module - starting test...\n');
  const test = new HybridLighthouseTest();
  test.run().catch(error => {
    console.error('Test execution failed:', error);
    process.exit(1);
  });
} else {
  console.log(':information_source:  Not running as main module (imported as library)');
  console.log(':bulb: TIP: To run this test directly, add this to the end of the file:');
  console.log('   new HybridLighthouseTest().run();\n');
}

export { HybridLighthouseTest };