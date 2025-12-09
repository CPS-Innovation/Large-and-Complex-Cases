import playwright from 'playwright';
import lighthouse from 'lighthouse';
import fs from 'fs-extra';
import * as chromeLauncher from 'chrome-launcher';
import { createAuthManager } from './auth-utils.ts';


const baseUrl = process.env.BASE_URL || 'http://localhost:5173';
const aadUsername = process.env.AZURE_AD_USERNAME || '';
const aadPassword = process.env.AZURE_AD_PASSWORD || '';

(async () => {

  const browserServer = await playwright.chromium.launch({
    headless: false,
    args: [ '-- remote-debugging-port=9222'],
  });

  const context = await browserServer.newContext();
  const authManager = createAuthManager();
        
  const tokens = await authManager.authenticate();
  
  // Apply authentication to browser context
  await authManager.setAuthContext(context, tokens);

  const page = await context.newPage();

  // Navigate and perform actions
  await page.goto(baseUrl);
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

  const chrome = await chromeLauncher.launch({ port: 9222 });

  const options =  { 
    port: chrome.port, 
    disableStorageReset: true, 
    onlyCategories: ['performance', 'accessibility', 'best-practices'],
    output: 'html',
  }

  const result = await lighthouse(page.url(), options);

  const reportHtml = result?.report;

  fs.writeFileSync('lh-report.html', reportHtml);
  console.log('Performance Score:', result?.lhr.categories.performance.score);
  console.log('Accessibility Score:', result?.lhr.categories.accessibility.score);
  console.log('Best Practice Score:', result?.lhr.categories['best-practices'].score);

  await browserServer.close();
})();