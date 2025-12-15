import playwright from 'playwright';
import lighthouse from 'lighthouse';
import fs from 'fs-extra';
import path from 'path';
import { fileURLToPath } from 'url';
import * as chromeLauncher from 'chrome-launcher';
import { createAuthManager } from './auth-utils.ts';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const baseUrl = process.env.BASE_URL || 'http://localhost:5173';
const aadUsername = process.env.AZURE_AD_USERNAME || '';
const aadPassword = process.env.AZURE_AD_PASSWORD || '';
const headless: boolean = process.env.HEADLESS?.trim().toLowerCase() !== 'false'; 

const userDataDir = path.resolve('.tmp/lh-profile');

async function run() {
  const browser = await playwright.chromium.launchPersistentContext(userDataDir, {
    channel: 'chrome',
    headless,
    args: ['--remote-debugging-port=9222'],
  });

  const authManager = createAuthManager();
        
  const tokens = await authManager.authenticate();
  
  // Apply authentication to browser context
  await authManager.setAuthContext(browser, tokens);

  const page = await browser.newPage();

  // Navigate and perform actions
  await page.goto(baseUrl);
  try {
    // Check if userDataDir contains an authenticated context
    await page.waitForSelector('[data-testid="div-ad-username"]', { timeout: 10000 });
    console.log('✓ User already authenticated. Skipping interactive authentication');
  } catch {
    await page.waitForURL('**/login.microsoftonline.com/**', { timeout: 15000 });

    await page.locator('input[type="email"]').fill(aadUsername);
    await page.locator('input[type="submit"]').click();

    await page.waitForSelector('input[type="password"]', { timeout: 10000 });
    await page.locator('input[type="password"]').fill(aadPassword);
    await page.locator('input[type="submit"]').click();

    try {
      await page.locator('input[type="submit"][value="Yes"]').click({ timeout: 3000 });
    } catch {
      // ignore if not shown
    }

    await page.waitForURL(baseUrl, { timeout: 20000 });
    console.log('✓ Interactive authentication completed');
  }

  await new Promise(r => setTimeout(r, 2000));

  let exitCode = 0;
  try {
    const options =  { 
      port: 9222, 
      disableStorageReset: true, 
      logLevel: 'error' as const,
      onlyCategories: ['performance', 'accessibility', 'best-practices'],
      output: 'html' as const,
      chromeFlags: ['--auto-select-certificate-for-urls']
    }

    const result = await lighthouse(baseUrl, options);
    
    const lhr = result?.lhr;

    if (lhr?.runtimeError) {
      throw new Error(`runtimeError: ${JSON.stringify(lhr.runtimeError)}`);
    }
    if (lhr?.runWarnings?.length) {
      throw new Error(`runWarnings: ${JSON.stringify(lhr.runWarnings)}`);
    }

    const reportHtml = result?.report as string;

    fs.writeFileSync(path.join(__dirname, '../test-results/lh-report.html'), reportHtml);
    console.log('Performance Score:', lhr?.categories.performance.score);
    console.log('Accessibility Score:', lhr?.categories.accessibility.score);
    console.log('Best Practice Score:', lhr?.categories['best-practices'].score);
  } catch (err: any) {
    const msg = String(err?.stack || err?.message || err);
    console.error('❌ Lighthouse failed:', msg);
    exitCode = 1;
  } finally {
    await browser.close();
  }
  process.exit(exitCode);
}

run();