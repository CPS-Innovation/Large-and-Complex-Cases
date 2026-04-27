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
  fixtures/
    setup-helper.ts                   # Register-case API + login (used by setup project)
    setup-helper-default.ts           # Default-mode setup (upload to pre-connected case)
    test-fixtures-register-case.ts    # Shared register-case fixture (per-test file upload)
    test-fixtures-default.ts          # Default mode fixture (100MB x 1)
    test-fixtures-default-large.ts    # Default mode fixture (200MB x 1)
  helpers/
    auth-api.ts                       # Azure AD + CMS authentication
    case-api.ts                       # Case registration API
    egress-api.ts                     # Egress workspace, upload, folder management
    env-config.ts                     # Environment variable loader
    constants.ts                      # Non-sensitive shared Egress IDs
    types.ts                          # TypeScript type definitions
  pages/                              # Page Object Models
  tests/
    register-case.setup.ts            # Setup project: register + connect once per run
    *-default.spec.ts                 # Default-mode specs (DEFAULT_WORKSPACE_ID)
    *.spec.ts                         # Register-case specs (consume setup state)
  playwright.config.ts                # Three projects: setup, register-case, default-mode
  .env.template
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

## Environment Profiles

`playwright.config.ts` loads the env file named `.env.${ENVIRONMENT}` at
startup, defaulting to `.env.local` when `ENVIRONMENT` is unset. That means
you can keep multiple profiles side-by-side (e.g. `.env.local`, `.env.dev`,
`.env.ci`) and switch between them per invocation.

Examples:

```bash
# Default — reads .env.local
npm run e2e:existing-case

# Read .env.dev instead
ENVIRONMENT=dev npm run e2e:existing-case

# Register-case flow against a CI profile
ENVIRONMENT=ci npm run e2e:register-case
```

Only `.env.template` is tracked in git; every `.env.<name>` file is ignored
by the root `.gitignore` to avoid accidental secret commits.

## Running Tests

```bash
# Run with Playwright UI mode
npm run e2e

# Run all tests (headless)
npm run e2e:ci

# Run only default mode tests (existing case, faster)
npm run e2e:existing-case

# Run only default mode tests in Playwright UI mode
npm run e2e:existing-case:ui

# Run only register case tests (full flow)
npm run e2e:register-case

# Run only register case tests in Playwright UI mode
npm run e2e:register-case:ui

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

Setup and test execution are split across two Playwright projects so the
expensive register + connect flow runs once per suite rather than once per
spec.

1. **`register-case-setup` project** (runs once, before any register-case spec):
   - Authenticate with Egress API, create a unique workspace, add test user,
     upload a 1MB seed file
   - Get Azure AD + CMS auth tokens and register a fresh case
   - Browser login (Tactical + Azure AD), search the case by URN, connect
     Egress workspace, connect NetApp folder
   - Persist `storageState` to `.auth/register-case.json` and shared case
     info (`{workspace, caseUrn}`) to `.state/register-case.json`

2. **Per-spec fixture** (`test-fixtures-register-case.ts`):
   - Load shared state from `.state/register-case.json`
   - Upload the spec's sized files (configurable via `test.use({ testOptions })`)
     into the shared workspace
   - Browser starts with the persisted `storageState`, so login + connect
     steps are skipped

3. **Test steps** (in the browser):
   - Search for the pre-connected case by URN
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

## Cleanup / Test Data Hygiene

Cleanup is driven by the suite itself, not ops. The test fixtures delete
per-test files on success, and a dedicated teardown project sweeps stale
workspaces on every register-case run.

### Per-test file teardown

Both fixtures delete the files they uploaded after `use()` returns, but
**only when `testInfo.status === "passed"`**. On failure the files are left
in their dated subfolder for post-mortem inspection.

- Register-case: `fixtures/test-fixtures-register-case.ts` calls
  `deleteFiles()` with the id array captured from `uploadFile()`.
- Default: `fixtures/test-fixtures-default.ts` and
  `fixtures/test-fixtures-default-large.ts` do the same, against
  `DEFAULT_WORKSPACE_ID`. The **workspace itself is never deleted** from
  default-mode teardown — it's shared.

Files uploaded by passing tests therefore disappear at run end; files from
failing tests stay put, grouped under:

- `4. Served Evidence/e2e-<spec>-<random>/` (register-case, source side)
- `2. Counsel only/e2e-<spec>-<random>/` (register-case, NetApp→Egress dest)
- `4. Served Evidence/e2e-YYYY-MM-DDTHH-MM-SS/` (default mode, source side)
- `2. Counsel only/e2e-YYYY-MM-DDTHH-MM-SS/` (default mode, NetApp→Egress dest)

### Register-case run-end workspace sweep

The `register-case-teardown` project (wired via `teardown:` on the setup
project in `playwright.config.ts`) runs after all dependent tests finish.
It applies a **rolling 24-hour sweep**:

1. Lists every workspace whose name matches `AUTOMATION-TESTING*` via the
   Egress `view=full` endpoint (`listAutomationWorkspaces` in
   `helpers/egress-api.ts`).
2. Filters to those with `date_created < now - 24h`.
3. Excludes the current run's workspace id (from `.state/register-case.json`)
   and `DEFAULT_WORKSPACE_ID` — these are **never deleted** by the sweep.
4. Calls `deleteWorkspace()` best-effort on each survivor (warns on failure,
   never throws, so a flaky delete won't mask test success).
5. Removes `.auth/` and `.state/` files so the next run starts clean.

Consequence: today's register-case workspace stays alive for ~24h after its
run finishes (useful for debugging a failed run), then gets swept by the
first run of the following day.

### What still isn't automated

- **Registered cases** — CMS/DDEI have no programmatic archive. Registered
  cases accumulate and need an ops-side archive process.
- **NetApp source files** — `netapp-to-egress-copy.spec.ts` and
  `netapp-to-egress-move.spec.ts` read whatever newest file is on NetApp;
  the suite doesn't upload to NetApp, so nothing to delete there.
- **Empty subfolders** — `deleteFiles` removes files but leaves the
  `e2e-*` folders behind. Cheap clutter; periodic ops prune if needed.
