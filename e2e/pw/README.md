# LCC Playwright E2E Tests

End-to-end tests for the **Large and Complex Cases (LCC)** file transfer system, built with [Playwright](https://playwright.dev/).

## Test Suite

| Test | Files | Description |
|------|-------|-------------|
| Egress to NetApp Copy | 100MB x 1 | Copy a single file from Egress to NetApp |
| Egress to NetApp Copy - Large | 200MB x 1 | Copy a large file from Egress to NetApp |
| Egress to NetApp Copy - Multifile | 50MB x 3 | Copy multiple files from Egress to NetApp |
| Egress to NetApp Move | 100MB x 1 | Move files from Egress to NetApp (skipped) |
| NetApp to Egress Copy | 100MB x 1 | Copy a file from NetApp to Egress |
| Login Only | 100MB x 1 | Full login flow and workspace/case setup verification |

## Project Structure

```
e2e/pw/
  fixtures/           # Playwright test fixtures (test setup/teardown)
    setup-helper.ts       # Shared setup logic (workspace, upload, case, login)
    test-fixtures.ts      # Default fixture (100MB x 1 from env vars)
    test-fixtures-large.ts    # Large file fixture (200MB x 1)
    test-fixtures-multifile.ts # Multi-file fixture (50MB x 3)
  helpers/            # API helpers and utilities
    auth-api.ts           # Azure AD + CMS authentication
    case-api.ts           # Case registration API
    egress-api.ts         # Egress workspace, upload, and file management
    env-config.ts         # Environment variable loader
    types.ts              # TypeScript type definitions
  pages/              # Page Object Models
    ActivityLogTab.ts
    AzureADLoginPage.ts
    CaseManagementPage.ts
    CaseSearchPage.ts
    EgressConfirmationPage.ts
    EgressConnectPage.ts
    NetAppConfirmationPage.ts
    NetAppConnectPage.ts
    SearchResultsPage.ts
    TacticalLoginPage.ts
    TransferMaterialsTab.ts
  tests/              # Test spec files
  pipelines/          # Azure DevOps pipeline definitions
  playwright.config.ts
  .env.template       # Environment variable template
```

## Prerequisites

- Node.js 20+
- Playwright browsers: `npx playwright install --with-deps chromium`
- Valid Azure AD, CMS, DDEI, and Egress credentials

## Setup

1. Install dependencies:
   ```bash
   npm ci
   ```

2. Install Playwright browsers:
   ```bash
   npx playwright install --with-deps chromium
   ```

3. Create a local environment file:
   ```bash
   cp .env.template .env.local
   ```

4. Fill in credentials in `.env.local` (this file is git-ignored).

## Environment Variables

| Variable | Description |
|----------|-------------|
| `BASE_URL` | LCC UI URL |
| `CMS_LOGIN_PAGE` | Tactical login endpoint |
| `CASE_API_BASE_URL` | Case Management API |
| `DDEI_BASE_URL` | DDEI API |
| `EGRESS_BASE_URL` | Egress API |
| `TENANT_ID` | Azure AD tenant ID |
| `CLIENT_ID` | Azure AD client ID |
| `E2E_AD_USER` | Azure AD test user email |
| `E2E_AD_PASSWORD` | Azure AD test user password |
| `CMS_USERNAME` | CMS username |
| `CMS_PASSWORD` | CMS password |
| `DDEI_ACCESS_KEY` | DDEI function key |
| `EGRESS_SERVICE_ACCOUNT_AUTH` | Egress service account (Base64) |
| `EGRESS_TEMPLATE_ID` | Egress workspace template ID |
| `EGRESS_ADMIN_ROLE_ID` | Egress admin role ID |
| `TEST_FILE_SIZE_MB` | Test file size in MB (default: 100) |
| `TEST_FILE_COUNT` | Number of test files to upload (default: 1) |

## Running Tests

```bash
# Run all tests (headed, sequential)
npm run e2e:headed

# Run all tests (headless, for CI)
npm run e2e:ci

# Run with Playwright UI mode
npm run e2e

# Run a specific test
npx playwright test egress-to-netapp-copy.spec.ts

# Run with headed browser
npx playwright test egress-to-netapp-copy.spec.ts --headed
```

## Reports

```bash
# View HTML report from last run
npm run report

# Run tests and generate HTML report
npm run report:generate

# Run tests and output JUnit XML (for CI/CD)
npm run report:junit
```

Reports are saved to `./playwright-report/`. In CI/CD, reports are published as pipeline artifacts.

## Failure Artifacts

On test failure, the following are automatically captured in `./test-results/`:

- **Screenshot** -- taken at the exact point of failure
- **Video** -- full recording of the failed test (`.webm`)
- **Trace** -- step-by-step replay with DOM snapshots, network requests, and console logs

To view a trace:
```bash
npx playwright show-trace test-results/<test-folder>/trace.zip
```

## Test Flow

Each test follows this flow:

1. **Fixture setup** (automated, runs before each test):
   - Authenticate with Egress API
   - Create a unique Egress workspace
   - Upload test file(s) via chunked upload
   - Get Azure AD + CMS auth tokens
   - Register a fresh case via the Case API
   - Browser login (Tactical + Azure AD)

2. **Test steps** (in the browser):
   - Search for the case by URN
   - Connect Egress workspace to the case
   - Connect NetApp folder to the case
   - Navigate to Transfer Materials tab
   - Select source files and initiate Copy/Move
   - Confirm transfer and wait for completion
   - Verify transfer in Activity Log
   - Download activity log CSV

## CI/CD Pipeline

The Azure DevOps pipeline (`pipelines/e2e-tests.yml`) runs on weekday mornings at 6:00 AM and supports:

| Parameter | Default | Options |
|-----------|---------|---------|
| Environment | `dev` | `dev`, `qa` |
| File size (MB) | `100` | Any number |
| File count | `1` | Any number |
| Tests to run | `all` | `all`, `egress-to-netapp-copy`, `egress-to-netapp-move`, `netapp-to-egress-copy` |

Secrets are loaded from the Azure DevOps variable group `lacc-e2e-secrets-<environment>`.
