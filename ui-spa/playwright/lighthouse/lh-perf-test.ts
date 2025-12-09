/**
 * Lighthouse Performance Test for File Transfer Stuff
 * 
 * SCOPE: This test measures PAGE LOAD PERFORMANCE ONLY
 * - Initial page render time
 * - Lighthouse metrics (LCP, FCP, CLS)
 * - Data presence verification
 * 
 * KNOWN LIMITATIONS:
 * - Lighthouse runs in separate browser instance without authentication
 * - May produce inaccurate results for authenticated pages
 * - Playwright metrics are more reliable for this use case
 */

import { chromium} from 'playwright';
import type { Browser, BrowserContext, Page } from 'playwright';
import lighthouse from 'lighthouse';
import type { RunnerResult } from 'lighthouse';
// import * as chromeLauncher from 'chrome-launcher';
import * as fs from 'fs-extra';
import * as path from 'path';
import chalk from 'chalk';
import moment from 'moment';
import { fileURLToPath } from 'url'
import { createAuthManager, validateAuthEnvironment, interactiveAdAuth } from './auth-utils.ts';
import { Config, validateConfig } from './config.ts';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

interface PerformanceResult {
  operationName: string;
  url: string;
  timestamp: string;
  lighthouse: {
    performance: number;
    accessibility: number;
    bestPractices: number;
    metrics: {
      firstContentfulPaint: number;
      largestContentfulPaint: number;
      cumulativeLayoutShift: number;
      firstInputDelay: number;
    };
  };
  // playwright: {
  //   pageLoadTime: number;
  //   domContentLoaded: number;
  // };
  // dataVerification: {
  //   hasEgressConnection: boolean;
  //   hasNetAppConnection: boolean;
  //   egressItemCount?: number;
  //   netAppItemCount?: number;
  // };
  passed: boolean;
  // warnings: string[];
}

class SimpleLighthouseTest {
  private browser: Browser | null = null;
  private context: BrowserContext | null = null;
  private page: Page | null = null;

  async run(): Promise<void> {
    console.log(chalk.blue('🚀 Starting Lighthouse Performance Tests with Real Authentication'));
    
    try {
      // Validate configuration
      validateAuthEnvironment();
      validateConfig();
      
      console.log(`→ Testing ${Config.cases.length} cases: ${Config.cases.join(', ')}`);
      
      const results: PerformanceResult[] = [];
      
      // Test each configured case
      for (const operationName of Config.cases) {
        console.log(chalk.yellow(`\n🔍 Testing Case for Operation ${operationName}`));
        
        await this.initializeBrowser();
        const authManager = createAuthManager();
        
        // Authenticate
        const tokens = await authManager.authenticate();
        
        // Apply authentication to browser context
        if (this.context) {
          console.log('DEBUG: Setting context')
          await authManager.setAuthContext(this.context, tokens);
        }

        // this.context
        
        // Run performance test for this case
        const result = await this.testCasePerformance(operationName);
        results.push(result);
        
        await this.cleanup();
      }
      
      // Generate report
      await this.generateReport(results);
      
      console.log(chalk.green('\n✅ All tests completed successfully'));
      
    } catch (error) {
      console.error(chalk.red('❌ Test failed:'), error);
      throw error;
    }
  }

  private async initializeBrowser(): Promise<void> {
    console.log('→ Launching browser...');

    const userAgent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'

    const storageState = await interactiveAdAuth({
      baseUrl: Config.baseUrl,
      username: Config.aadUsername,
      password: Config.aadPassword,
      headless: Config.headless,
      screenSize: Config.screenSize,
      userAgent,
    });

    this.browser = await chromium.launch({
      headless: Config.headless,
      args: [
        '--no-sandbox',
        '--disable-dev-shm-usage',
        '--disable-gpu',
        '--remote-debugging-port=9222'
      ],
    });

    this.context = await this.browser.newContext({
      viewport: Config.screenSize,
      userAgent,
      storageState
    });

    this.page = await this.context.newPage();
    console.log('✓ Browser initialized');
  }

  private async testCasePerformance(operationName: string): Promise<PerformanceResult> {
    if (!this.page) throw new Error('Page not initialized');

    const testUrl = Config.baseUrl;
    
    // const startTime = Date.now();
    
    await this.page.goto(testUrl, { waitUntil: 'networkidle' }); 
    
    // Wait for key elements
    await this.page.waitForSelector('[data-testid="radio-search-operation-name"]', { timeout: 10000 });
    await this.page.locator('[data-testid="radio-search-operation-name"]').click();
    await this.page.locator('[data-testid="search-operation-name"]').fill(operationName);
    await this.page.getByRole('button', { name: 'Search' }).click();

    // // Verify both Egress and NetApp sections are present
    // const hasEgressSection = await this.page.locator('[data-testid="egress-table-wrapper"]').isVisible();
    // const hasNetAppSection = await this.page.locator('[data-testid="netapp-table-wrapper"]').isVisible();
    
    // if (!hasEgressSection || !hasNetAppSection) {
    //   throw new Error(
    //     `Case ${operationName} missing required connections ` +
    //     `(Egress: ${hasEgressSection}, NetApp: ${hasNetAppSection}). ` +
    //     `Ensure the test case has both Egress workspace and NetApp folder connected.`
    //   );
    // }
    
    // // Wait for actual data to load (not just empty tables)
    // try {
    //   await this.page.waitForSelector('[data-testid="egress-table-wrapper"] table tbody tr', { 
    //     timeout: 15000 
    //   });
    //   const egressRowCount = await this.page.locator('[data-testid="egress-table-wrapper"] table tbody tr').count();
    //   console.log(`   ✓ Egress data loaded: ${egressRowCount} items`);
    // } catch (error) {
    //   console.warn(`   ⚠ Warning: No Egress data found for case ${operationName}`);
    // }
    
    // try {
    //   await this.page.waitForSelector('[data-testid="netapp-table-wrapper"] table tbody tr', { 
    //     timeout: 15000 
    //   });
    //   const netAppRowCount = await this.page.locator('[data-testid="netapp-table-wrapper"] table tbody tr').count();
    //   console.log(`   ✓ NetApp data loaded: ${netAppRowCount} items`);
    // } catch (error) {
    //   console.warn(`   ⚠ Warning: No NetApp data found for case ${operationName}`);
    // }
    
    // const pageLoadTime = Date.now() - startTime;
    
    // // Capture Playwright metrics
    // const playwrightMetrics = await this.page.evaluate(() => {
    //   const perfData = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
    //   return {
    //     pageLoadTime: perfData.loadEventEnd - perfData.navigationStart,
    //     domContentLoaded: perfData.domContentLoadedEventEnd - perfData.navigationStart
    //   };
    // });
    
    // // Capture data verification metrics
    // const egressItemCount = await this.page.locator('[data-testid="egress-table-wrapper"] table tbody tr').count().catch(() => 0);
    // const netAppItemCount = await this.page.locator('[data-testid="netapp-table-wrapper"] table tbody tr').count().catch(() => 0);
    
    // const warnings: string[] = [];
    // if (egressItemCount === 0) warnings.push('No Egress data loaded');
    // if (netAppItemCount === 0) warnings.push('No NetApp data loaded');
    
    // Run Lighthouse audit (Note: This launches a new browser without auth)
    console.log('→ Running Lighthouse audit...');
    const lighthouseResults = await this.runLighthouseAudit(testUrl);
    await this.page.pause();
    
    const result: PerformanceResult = {
      operationName: operationName,
      url: testUrl,
      timestamp: moment().toISOString(),
      lighthouse: lighthouseResults,
      // playwright: playwrightMetrics,
      // dataVerification: {
      //   hasEgressConnection: hasEgressSection,
      //   hasNetAppConnection: hasNetAppSection,
      //   egressItemCount: egressItemCount > 0 ? egressItemCount : undefined,
      //   netAppItemCount: netAppItemCount > 0 ? netAppItemCount : undefined
      // },
      passed: this.validatePerformance(lighthouseResults),
      // warnings
    };
    
    this.logResult(result);
    return result;
  }

  private async runLighthouseAudit(url: string): Promise<any> {
    // const chrome = await chromeLauncher.launch({
    //   // chromeFlags: ['--headless', '--no-sandbox', '--disable-dev-shm-usage']
    // });
    
    try {
      const runnerResult = (await lighthouse(url, {
        port: 9222,
        disableStorageReset: true,
        onlyCategories: ['performance', 'accessibility', 'best-practices'],
        skipAudits: ['uses-http2', 'uses-text-compression'],
        output: 'json'
    })) as RunnerResult;

      return {
        performance: (runnerResult.lhr.categories.performance?.score ?? 0) * 100,
        accessibility: (runnerResult.lhr.categories.accessibility.score ?? 0) * 100,
        bestPractices: (runnerResult.lhr.categories['best-practices'].score ?? 0) * 100,
        metrics: {
          firstContentfulPaint: runnerResult.lhr.audits['first-contentful-paint'].numericValue,
          largestContentfulPaint: runnerResult.lhr.audits['largest-contentful-paint'].numericValue,
          cumulativeLayoutShift: runnerResult.lhr.audits['cumulative-layout-shift'].numericValue,
          firstInputDelay: runnerResult.lhr.audits['max-potential-fid'].numericValue
        }
      };
    } finally {
      // await browser.close();
    }
  }

  private validatePerformance(results: any): boolean {
    // Basic performance validation
    const performancePassed = results.performance >= 80;
    const lcpPassed = results.metrics.largestContentfulPaint <= Config.performanceBudget.lcp;
    const clsPassed = results.metrics.cumulativeLayoutShift <= Config.performanceBudget.cls;
    
    return performancePassed && lcpPassed && clsPassed;
  }

  private logResult(result: PerformanceResult): void {
    console.log(chalk.blue(`   Case ${result.operationName}:`));
    console.log(`     Performance: ${result.lighthouse.performance.toFixed(0)}/100`);
    console.log(`     Accessibility: ${result.lighthouse.accessibility.toFixed(0)}/100`);
    // console.log(`     LCP: ${result.lighthouse.metrics.largestContentfulPaint.toFixed(0)}ms`);
    // console.log(`     Data: Egress=${result.dataVerification.egressItemCount || 0}, NetApp=${result.dataVerification.netAppItemCount || 0}`);
    // if (result.warnings.length > 0) {
    //   console.log(chalk.yellow(`     Warnings: ${result.warnings.join(', ')}`));
    // }
    console.log(`     Status: ${result.passed ? chalk.green('✅ Passed') : chalk.red('❌ Failed')}`);
  }

  private async generateReport(results: PerformanceResult[]): Promise<void> {
    const timestamp = moment().format('YYYY-MM-DD_HH-mm-ss');
    const outputDir = path.join(__dirname, 'test-results');
    await fs.ensureDir(outputDir);
    
    // Generate JSON report
    const jsonReport = {
      timestamp: moment().toISOString(),
      testConfiguration: {
        baseUrl: Config.baseUrl,
        cases: Config.cases,
        performanceBudget: Config.performanceBudget
      },
      results,
      summary: {
        totalTests: results.length,
        passedTests: results.filter(r => r.passed).length,
        failedTests: results.filter(r => !r.passed).length,
        avgPerformance: results.reduce((sum, r) => sum + r.lighthouse.performance, 0) / results.length,
        avgAccessibility: results.reduce((sum, r) => sum + r.lighthouse.accessibility, 0) / results.length
      }
    };
    
    const jsonPath = path.join(outputDir, `lighthouse-results-${timestamp}.json`);
    await fs.writeJson(jsonPath, jsonReport, { spaces: 2 });
    
    // Generate simple markdown summary
    const markdownSummary = this.generateMarkdownSummary(results, jsonReport.summary);
    const mdPath = path.join(outputDir, `lighthouse-summary-${timestamp}.md`);
    await fs.writeFile(mdPath, markdownSummary);
    
    console.log(chalk.green.bold('\n📊 Reports Generated:'));
    console.log(chalk.gray(`   📄 JSON: ${jsonPath}`));
    console.log(chalk.gray(`   📄 Markdown: ${mdPath}`));
  }

  private generateMarkdownSummary(results: PerformanceResult[], summary: any): string {
    return `# Lighthouse Performance Test Results

## Test Summary
- **Timestamp**: ${moment().format('YYYY-MM-DD HH:mm:ss')}
- **Total Tests**: ${summary.totalTests}
- **Passed**: ${summary.passedTests}
- **Failed**: ${summary.failedTests}
- **Success Rate**: ${(summary.passedTests / summary.totalTests * 100).toFixed(1)}%

## Performance Summary
- **Average Performance Score**: ${summary.avgPerformance.toFixed(1)}/100
- **Average Accessibility Score**: ${summary.avgAccessibility.toFixed(1)}/100

## Detailed Results

${results.map(result => `
### Case ${result.operationName}
- **Performance**: ${result.lighthouse.performance.toFixed(0)}/100
- **Accessibility**: ${result.lighthouse.accessibility.toFixed(0)}/100
- **LCP**: ${result.lighthouse.metrics.largestContentfulPaint.toFixed(0)}ms
- **CLS**: ${result.lighthouse.metrics.cumulativeLayoutShift.toFixed(3)}
- **Status**: ${result.passed ? '✅ Passed' : '❌ Failed'}
`).join('')}

## Configuration
- **Base URL**: ${Config.baseUrl}
- **Test Cases**: ${Config.cases.join(', ')}
- **Performance Budget (LCP)**: ${Config.performanceBudget.lcp}ms
`;
  }

  private async cleanup(): Promise<void> {
    if (this.page) {
      await this.page.close();
    }
    if (this.context) {
      await this.context.close();
    }
    if (this.browser) {
      await this.browser.close();
    }
  }
}

if (process.argv[1] === __filename) {
  const test = new SimpleLighthouseTest();
  test.run().catch(error => {
    console.error('Test execution failed:', error);
    process.exit(1);
  });
}

export { SimpleLighthouseTest };