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
| Egress to NetApp Move | 100MB x 1 | ✓ | ✓ |
| NetApp to Egress Copy | 100MB x 1 | ✓ (uses seeded fixture) | ✓ (sort + row 0) |
| Full Flow (login, search, connect) | 100MB x 1 | -- | ✓ |

NetApp -> Egress Move is descoped — the product UI doesn't expose a Move
button in that direction (`EgressFolderContainer.tsx` has no Move
references; only `NetAppFolderContainer.tsx` does, gated on the
`transferMove` feature flag, which renders Move in the Egress -> NetApp
direction only).

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
    egress-api.ts                     # Egress workspace, upload, folder, delete helpers
    netapp-api.ts                     # NetApp file delete (per-test cleanup, default mode)
    env-config.ts                     # Environment variable loader
    constants.ts                      # NETAPP_FIXTURE_FILENAME, REGISTER_CASE_NETAPP_FOLDER, Egress IDs
    register-case-state.ts            # Shared state file paths for the setup project
    types.ts                          # TypeScript type definitions
  pages/                              # Page Object Models
  tests/
    register-case.setup.ts            # Setup project: register + connect once per run
    register-case.teardown.ts         # 24h workspace sweep, run after register-case-tests
    seed-netapp-fixture.setup.ts      # Opt-in NetApp source seed (RUN_SEED=1 to fire)
    *-default.spec.ts                 # Default-mode specs (DEFAULT_WORKSPACE_ID)
    *.spec.ts                         # Register-case specs (consume setup state)
  scripts/
    upload-to-workspace.ts            # Standalone Egress upload tool (ad-hoc seeding)
  playwright.config.ts                # 5 projects: register-case-setup,
                                      # register-case-teardown, register-case-tests,
                                      # default-mode-tests, seed-netapp-fixture
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

5. **Seed the canonical NetApp fixture** — required only for the
   default-mode `netapp-to-egress-copy-default.spec.ts` (see "Required
   NetApp fixture" below). Register-case mode doesn't need it.

## Default-mode workspace + NetApp folder convention

For existing-case (default) mode, the Egress workspace and the connected
NetApp folder both use the name **`existingCaseAutomation`**. Provision
the pair once per environment and reuse it across all default-mode
runs. Set `DEFAULT_WORKSPACE_NAME` and `NETAPP_OPERATION_NAME` to
`existingCaseAutomation`, and fill in the corresponding workspace id +
case id/urn in the other `DEFAULT_*` variables.

Register-case mode is independent: it creates a fresh
`AUTOMATION-TESTING<N>-<random>` workspace per run and connects to the
shared `Automation-Testing` NetApp folder (hardcoded in
`helpers/constants.ts` as `REGISTER_CASE_NETAPP_FOLDER`).
`NETAPP_OPERATION_NAME` is not read by the register-case fixture.

## Required NetApp fixture

The default-mode `netapp-to-egress-copy-default.spec.ts` needs a
deterministic source file. Picking the newest row of NetApp's listing
isn't viable in default mode: the panel doesn't toggle descending date
sort, has no search/filter, and pagination isn't triggered by Playwright
scrolls — so a just-uploaded file is buried, and the existing
accumulated `generated-100MB-2026-01-20-12-23-11.txt` at row 0 hits a
backend transfer-rejection state.

The contract: a single canonical file lives at
`existingCaseAutomation/lcc-e2e-fixture-source.txt` on the shared drive.
The default-mode spec selects this file by exact name and fails fast
with a clear "fixture missing" message if it isn't present.

**Register-case mode does not need the fixture.** Its destination
Egress workspace is fresh per run, so there's no name-collision concern;
the register-case `netapp-to-egress-copy.spec.ts` keeps its sort + row-0
selection.

### NetApp source pre-condition (register-case mode)

`netapp-to-egress-copy.spec.ts` requires `Automation-Testing/` (the
shared NetApp folder, `REGISTER_CASE_NETAPP_FOLDER`) to contain at
least one file. Always satisfied today by accumulated test artefacts;
on a brand-new environment, drop any file in there first.

Seed it once per environment (opt-in — the seed project skips itself
unless `RUN_SEED=1` so the default `npx playwright test` invocation
doesn't repeatedly try to re-seed):

```bash
RUN_SEED=1 npx playwright test --project=seed-netapp-fixture
```

The seed (a Playwright setup test, not a plain script) uploads a 1MB
`.txt` to Egress via the Egress API, drives the LCC UI to do an
Egress -> NetApp copy of it, then deletes the Egress copy — leaving
only the NetApp side. Idempotent: re-running while the fixture already
exists is a no-op. The LCC backend's destination duplicate-file check
rejects the second copy attempt with "Some files already exist in the
destination folder", and the seed catches that error path explicitly
(see `tests/seed-netapp-fixture.setup.ts`).

The fixture's name pattern (`lcc-e2e-fixture-*`) is intentionally
distinct from the per-test `generated-100MB-*` artefacts, so the
automated cleanup helpers (`deleteNetAppFile`, 24h workspace sweep) will
never remove it.

Required env: `LCC_API_BASE_URL`, `NETAPP_OPERATION_NAME`,
`DEFAULT_WORKSPACE_ID`, `DEFAULT_CASE_ID`, `DEFAULT_CASE_URN`, plus the
standard auth set.

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
| `LCC_API_BASE_URL` | LCC backend URL — used by NetApp file teardown + disassociate | Default mode (recommended); register-case (recommended) |
| `NETAPP_OPERATION_NAME` | Connected NetApp folder for default mode | Default mode (recommended) |
| `LCC_API_CLIENT_ID` | LCC API app registration client id (client-credentials flow) for case-disassociate at register-case teardown | Register-case (recommended) |
| `LCC_API_CLIENT_SECRET` | Secret for `LCC_API_CLIENT_ID` | Register-case (recommended) |

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
   - Persist shared case info (`{workspace, caseUrn, caseId}`) to
     `.state/register-case.json`. (storageState is also written to
     `.auth/register-case.json` but per-spec fixtures do not consume it
     today — see step 2.)

2. **Per-spec fixture** (`test-fixtures-register-case.ts`):
   - Load shared state from `.state/register-case.json`
   - Upload the spec's sized files (configurable via `test.use({ testOptions })`)
     into the shared workspace
   - Re-run the full tactical + Azure AD browser login (`browserLogin`)
     per test. The setup-saved `storageState` is intentionally not
     applied: tactical cookies age fast and the LCC app leaves the
     case-search radios disabled (and rejects `/api/v1/case-search` with
     HTTP 400) when tactical is stale. Re-logging is fast next to upload
     + transfer time and avoids that race. The case + Egress/NetApp
     connect work from setup is still skipped.

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
  `deleteFiles()` (Egress side) and `deleteNetAppFile()` (NetApp side
  against `REGISTER_CASE_NETAPP_FOLDER`) for every file uploaded.
- Default: `fixtures/test-fixtures-default.ts` and
  `fixtures/test-fixtures-default-large.ts` do the same, against
  `DEFAULT_WORKSPACE_ID` for Egress and `NETAPP_OPERATION_NAME` for
  NetApp. The **workspace itself is never deleted** from default-mode
  teardown — it's shared.

Both fixtures also clean the **Egress destination folder** —
`2. Counsel only/<uploadSubfolder>/` — by listing it via
`listEgressWorkspaceFilesByFolderId()` (using the folder id captured
from the `createFolder` response, since Egress's `?path=` query
silently returns 0 against subfolder paths) and bulk-deleting whatever
is there. This catches the file the LCC backend writes during
NetApp→Egress copies, whose basename is decided by the backend (so we
can't predict it from the source). Egress→NetApp specs don't write to
this folder, so the list returns empty and the call is a no-op for
those runs. For NetApp→Egress specs the listing polls every 3 s up to
~30 s — Egress's file index lags the LCC `transfer-complete` signal by
a few seconds, so the first list often returns empty even when the
copy succeeded.

The NetApp delete is best-effort: a NetApp -> Egress spec doesn't push
to NetApp, so the delete attempt 404s and is logged as a warning rather
than a failure. The endpoint is also disabled in production by the
backend (returns 403); leave `LCC_API_BASE_URL` unset for prod-pointing
runs to skip the call entirely.

Files uploaded by passing tests therefore disappear at run end; files from
failing tests stay put, grouped under:

- `4. Served Evidence/e2e-<spec>-<random>/` (register-case, source side)
- `2. Counsel only/e2e-<spec>-<random>/` (register-case, NetApp→Egress dest)
- `4. Served Evidence/e2e-YYYY-MM-DDTHH-MM-SS/` (default mode, source side)
- `2. Counsel only/e2e-YYYY-MM-DDTHH-MM-SS/` (default mode, NetApp→Egress dest)

### Register-case run-end case disassociation

Before the workspace sweep, the teardown disassociates the just-run
case from its NetApp connection so registered cases don't leave dangling
NetApp links behind. Endpoint:

```
DELETE /api/v1/netapp/connections?case-id={caseId}
```

Auth shape: **app-only AAD token** via the client-credentials grant
against the LCC API app registration (`LCC_API_CLIENT_ID` +
`LCC_API_CLIENT_SECRET`) — *not* the user-delegated tokens used
elsewhere in the suite. The endpoint rejects user-delegated tokens
with 401; verified empirically via `scripts/smoke-disassociate.ts`.

Default mode never invokes this — the only callsite is
`tests/register-case.teardown.ts:65`, which is gated by both
Playwright's `teardown:` hook (only fires after register-case-tests)
and a `STATE_FILE` existence check (only written by register-case
setup). The default-mode existing case stays connected to NetApp
across runs.

Best-effort: a non-2xx is logged as a warn and swallowed; the
subsequent workspace sweep still proceeds.

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

- **Registered cases** — CMS/DDEI have no programmatic archive.
  Registered cases accumulate and need an ops-side archive process.
- **The NetApp source fixture** — `lcc-e2e-fixture-source.txt` is
  intentionally exempt from cleanup helpers (its name pattern doesn't
  match `generated-100MB-*`) so it persists across runs. Re-seed via
  the `seed-netapp-fixture` opt-in project if it goes missing.
- **Empty subfolders** — `deleteFiles` removes files but leaves the
  `e2e-*` folders behind. Cheap clutter; periodic ops prune if needed.
