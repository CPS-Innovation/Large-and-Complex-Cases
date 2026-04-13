# LCC Playwright E2E Tests

End-to-end tests for the **Large and Complex Cases (LCC)** file transfer system, built with [Playwright](https://playwright.dev/).

## Test Suites

Tests run in two modes:

| Mode | Command | Description |
|------|---------|-------------|
| **Default (existing case)** | `npm run e2e:existing-case` | Uses a pre-existing case and workspace. Skips case registration, Egress/NetApp connection. Faster for iterative testing. |
| **Register case** | `npm run e2e:register-case` | Creates a new workspace, registers a fresh case, sets up all connections. Full end-to-end flow. |

### Test Matrix

| Test | Files | Default Mode | Register Case Mode |
|------|-------|--------------|--------------------|
| Egress to NetApp Copy | 100MB x 1 | ✓ | ✓ |
| Egress to NetApp Copy - Large | 200MB x 1 | ✓ | ✓ |
| Egress to NetApp Copy - Multifile | 50MB x 3 | -- | ✓ |
| Egress to NetApp Move | 100MB x 1 | skipped | skipped |
| NetApp to Egress Copy | 100MB x 1 | ✓ | ✓ |
| Full Flow (login, search, connect) | 100MB x 1 | -- | ✓ |

## Project Structure

```
e2e/pw/
  fixtures/               # Playwright test fixtures (test setup/teardown)
    setup-helper.ts            # Full setup (workspace, upload, case registration, login)
    setup-helper-default.ts    # Default mode setup (upload to existing workspace, login)
    test-fixtures.ts           # Register case fixture (100MB x 1)
    test-fixtures-default.ts   # Default mode fixture (100MB x 1)
    test-fixtures-large.ts     # Register case large file fixture (200MB x 1)
    test-fixtures-default-large.ts # Default mode large file fixture (200MB x 1)
    test-fixtures-multifile.ts # Multi-file fixture (50MB x 3)
  helpers/                # API helpers and utilities
    auth-api.ts                # Azure AD + CMS authentication
    case-api.ts                # Case registration API
    egress-api.ts              # Egress workspace, upload, and file management
    env-config.ts              # Environment variable loader
    types.ts                   # TypeScript type definitions
  pages/                  # Page Object Models
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
  tests/                  # Test spec files
  playwright.config.ts
  .env.template           # Environment variable template
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

| Variable | Description | Required |
|----------|-------------|----------|
| `BASE_URL` | LCC UI URL | Yes |
| `CMS_LOGIN_PAGE` | Tactical login endpoint | Yes |
| `CASE_API_BASE_URL` | Case Management API | Yes |
| `DDEI_BASE_URL` | DDEI API | Yes |
| `EGRESS_BASE_URL` | Egress API | Yes |
| `TENANT_ID` | Azure AD tenant ID | Yes |
| `CLIENT_ID` | Azure AD client ID | Yes |
| `E2E_AD_USER` | Azure AD test user email | Yes |
| `E2E_AD_PASSWORD` | Azure AD test user password | Yes |
| `CMS_USERNAME` | CMS username | Yes |
| `CMS_PASSWORD` | CMS password | Yes |
| `DDEI_ACCESS_KEY` | DDEI function key | Yes |
| `EGRESS_SERVICE_ACCOUNT_AUTH` | Egress service account (Base64) | Yes |
| `EGRESS_TEMPLATE_ID` | Egress workspace template ID | No (has default) |
| `EGRESS_ADMIN_ROLE_ID` | Egress admin role ID | No (has default) |
| `TEST_FILE_SIZE_MB` | Test file size in MB | No (default: 100) |
| `TEST_FILE_COUNT` | Number of test files to upload | No (default: 1) |
| `DEFAULT_WORKSPACE_ID` | Pre-existing Egress workspace ID | Default mode only |
| `DEFAULT_WORKSPACE_NAME` | Pre-existing Egress workspace name | Default mode only |
| `DEFAULT_CASE_ID` | Pre-existing case ID | Default mode only |
| `DEFAULT_CASE_URN` | Pre-existing case URN | Default mode only |

## Running Tests

```bash
# Run with Playwright UI mode
npm run e2e

# Run all tests (headless)
npm run e2e:ci

# Run only default mode tests (existing case, faster)
npm run e2e:existing-case

# Run only register case tests (full flow)
npm run e2e:register-case

# Run all tests headed (sequential)
npm run e2e:headed

# Run a specific test
npx playwright test egress-to-netapp-copy-default

# Run with headed browser
npx playwright test egress-to-netapp-copy --headed
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

Reports are saved to `./playwright-report/`.

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

### Register Case Mode

1. **Fixture setup** (automated, before each test):
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

### Default Mode

1. **Fixture setup** (automated, before each test):
   - Authenticate with Egress API
   - Upload test file(s) to existing workspace
   - Browser login (Tactical + Azure AD)

2. **Test steps** (in the browser):
   - Search for the pre-existing case by URN
   - Navigate to Transfer Materials tab (connections already set up)
   - Select source files and initiate Copy/Move
   - Confirm transfer and wait for completion
   - Verify transfer in Activity Log

## Configuration

Tests run with `workers: 1` because all tests share the same Azure AD credentials -- parallel execution causes session conflicts.
