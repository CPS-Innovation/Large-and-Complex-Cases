# Local Development Guide

This document provides detailed instructions for setting up your local development environment and using the build/test scripts for the Large and Complex Cases (LACC) project.

---

## Quick Start (Recommended Order)

1. **Review Prerequisites**
2. **Clone the repository**
3. **Set up Environment Variables**
4. **Install Dependencies**
5. **Set up PostgreSQL database**
6. **Run the Application Locally**
7. **Access the UI at [http://localhost:5173](http://localhost:5173)**

---

## Prerequisites

- **.NET 8 SDK**: [Download here](https://dotnet.microsoft.com/download)
- **Node.js (v20+)**: [Download here](https://nodejs.org/)
- **npm**: Comes with Node.js
- **PowerShell**: Required to run the provided scripts (Windows, or [PowerShell Core](https://github.com/PowerShell/PowerShell) for Mac/Linux)
- **PostgreSQL**: [Download here](https://www.postgresql.org/download/) (version 13+ recommended)
  - For local database management, it is recommended to use [PGAdmin](https://www.pgadmin.org/) (a free graphical tool for PostgreSQL).
- **Azure Functions Core Tools**: Required to run backend function apps locally. [Install instructions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local):
  ```bash
  npm install -g azure-functions-core-tools@4 --unsafe-perm true
  ```

## Directory Structure

- `backend/` — .NET backend services and tests
- `ui-spa/` — React single-page application (SPA)
- `scripts/` — PowerShell scripts for local build, test, and packaging
- `build-output/` — Output directory for build artifacts (created by scripts)

## Environment Setup

1. **Clone the repository**
2. Ensure all prerequisites are installed and available in your PATH
3. Open a PowerShell terminal in the project root

## Environment Variables

### Backend (Function Apps)

Environment variables for backend services are managed via `local.settings.json` files. Use the provided `local.settings.example.json` as a template for each function app (e.g., `backend/CPS.ComplexCases.API/local.settings.example.json`).

**To use:**  
- Copy `local.settings.example.json` to `local.settings.json` in the same directory for both `CPS.ComplexCases.API` and `CPS.ComplexCases.FileTransfer.API`.
- Fill in the required secrets and connection strings.
- **Note:** Some secrets (e.g., Azure AD client IDs, access keys) may only be available from a team member or the Azure portal. If you do not have these, please contact your project lead or DevOps engineer.

### UI (React SPA)

The UI uses Vite and expects environment variables prefixed with `VITE_`. These are typically set in a `.env.local` file in the `ui-spa/` directory (Vite automatically loads `.env.local`).

**Common variables (see `src/config.ts`):**
```
VITE_GATEWAY_BASE_URL=<API base URL>
VITE_GATEWAY_SCOPE=<API scope>
VITE_MOCK_API_SOURCE=<mock API source>
VITE_CLIENT_ID=<Azure AD client id>
VITE_TENANT_ID=<Azure AD tenant id>
VITE_MOCK_AUTH=<true|false>
```
**Sample `.env.local` file:**
```
VITE_GATEWAY_BASE_URL=http://localhost:7071/api/
VITE_GATEWAY_SCOPE=user_impersonation
VITE_MOCK_API_SOURCE=http://localhost:7071/api/
VITE_CLIENT_ID=your-client-id
VITE_TENANT_ID=your-tenant-id
VITE_MOCK_AUTH=true
```
**To use:**  
- Create a `.env.local` file in `ui-spa/` and set the above variables as needed.

## Install Dependencies

### Backend

- **.NET 8 SDK**: [Download and install](https://dotnet.microsoft.com/download)
- **PostgreSQL**:  
  - Install [PostgreSQL](https://www.postgresql.org/download/) (version 13+ recommended).
  - Use [PGAdmin](https://www.pgadmin.org/) for easy local database management.
  - Ensure the PostgreSQL service is running.
  - Create a database and user as needed. Example SQL:
    ```sql
    CREATE DATABASE lacc;
    CREATE USER postgres WITH PASSWORD 'yourpassword';
    GRANT ALL PRIVILEGES ON DATABASE lacc TO postgres;
    ```
  - Update the `ConnectionStrings__CaseManagementDatastoreConnection` in your `local.settings.json` with the correct connection string, e.g.:
    ```
    Host=localhost;Port=5432;Database=lacc;Username=postgres;Password=yourpassword
    ```
  - The backend will automatically apply migrations if you use the `-RunMigration` flag with the scripts.

### UI

- **Node.js (v20+)**: [Download and install](https://nodejs.org/)
- **npm**: Comes with Node.js

## Running the Application Locally

> **First Run:** The first time you run the scripts, dependency installation and database migrations may take longer than subsequent runs.

### Backend (Function Apps)

> **Note:** Both `CPS.ComplexCases.API` (Main API) and `CPS.ComplexCases.FileTransfer.API` (FileTransfer API) must be running together for the application to function end-to-end. The UI only talks to the Main API, which acts as a gateway and calls the FileTransfer API for file operations. If the FileTransfer API is not running, file transfer features will not work.

1. **Restore, build, and test (optional):**
   ```powershell
   ./scripts/build-and-test-backend-local.ps1
   ```
   - Use `-SkipTests` to skip tests, `-PublishApps` to create deployment zips, and `-RunMigration` to apply DB migrations.

2. **Run the Function Apps locally:**
   - Open two terminals:
     - In the first terminal, start the Main API:
       ```powershell
       cd backend/CPS.ComplexCases.API
       func start
       ```
     - In the second terminal, start the FileTransfer API:
       ```powershell
       cd backend/CPS.ComplexCases.FileTransfer.API
       func start
       ```
     (Requires Azure Functions Core Tools installed.)

   - The default ports are set in `local.settings.json` (e.g., 7071 for Main API, 7072 for FileTransfer API).

### UI (React SPA)

1. **Install dependencies:**
   ```bash
   cd ui-spa
   npm ci
   ```

2. **Start the development server:**
   ```bash
   npm run dev
   ```
   - The app will be available at [http://localhost:5173](http://localhost:5173) by default.

3. **Build for production:**
   ```bash
   npm run build
   ```

4. **Preview the production build:**
   ```bash
   npm start
   ```

### Accessing the Application

- Once both backend APIs and the UI are running, open [http://localhost:5173](http://localhost:5173) in your browser.
- The UI communicates **only** with the **Main backend API** (`CPS.ComplexCases.API`, gateway API) running on port 7071. The Main API orchestrates business logic and, for file transfer operations, calls the **FileTransfer API** (`CPS.ComplexCases.FileTransfer.API`) running on port 7072. If the FileTransfer API is not running, any file transfer features in the UI or Main API will not work.

## Example Workflow

1. Set up your environment variables as described above.
2. Start PostgreSQL and ensure your connection string is correct.
3. Run the backend build/test script with migration:
   ```powershell
   ./scripts/build-and-test-backend-local.ps1 -RunMigration
   ```
4. In two terminals, start both backend function apps:
   ```powershell
   # Terminal 1
   cd backend/CPS.ComplexCases.API
   func start

   # Terminal 2
   cd backend/CPS.ComplexCases.FileTransfer.API
   func start
   ```
5. In another terminal, start the UI:
   ```bash
   cd ui-spa
   npm ci
   npm run dev
   ```
6. Open [http://localhost:5173](http://localhost:5173) in your browser.

## Running Tests Only

### Backend
- To run backend tests with coverage:
  ```powershell
  ./scripts/build-and-test-backend-local.ps1
  ```
  (Remove `-SkipTests` if present.)
- To run backend tests only (no build/package):
  - You can use `dotnet test` directly in the backend directory if needed.

### UI
- To run UI unit tests, coverage, and E2E tests, use the script:
  ```powershell
  ./scripts/build-and-test-ui-local.ps1
  ```
  - This will handle installing dependencies, running lint, unit tests with coverage, and E2E tests (unless skipped with flags).
- To run UI unit tests with coverage only (without E2E):
  ```bash
  cd ui-spa
  npm run coverage
  ```
- To run UI E2E tests (locally only):
  ```bash
  cd ui-spa
  npm run ui:e2e
  ```

## Troubleshooting

- If you see connection errors, check your PostgreSQL service and connection string.
- For missing environment variables, ensure your `.env.local` and `local.settings.json` files are present and correct.
- For Azure Functions, ensure you have [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) installed and available in your PATH.
- If you encounter issues on first run, try running `npm ci` in `ui-spa/` and `dotnet restore` in `backend/` manually.

## Additional Notes

- These scripts are designed for local development and testing. For CI/CD, see the Azure Pipeline YAML files referenced in the main README.
- Database migrations require a valid connection string and appropriate permissions.
- UI E2E tests are only supported locally at this time.

## Build & Test Scripts

All scripts are located in the `scripts/` directory. Each script provides help and usage information via comments and parameter definitions.

### 1. `build-and-test-backend-local.ps1`

Builds, tests, and packages the backend solution.

**Usage:**
```powershell
./scripts/build-and-test-backend-local.ps1 [-Configuration Release|Debug] [-BackendPath backend] [-OutputPath build-output] [-SkipTests] [-RunMigration] [-ConnectionString <string>] [-VerboseOutput] [-PublishApps]
```

**Key Options:**
- `-Configuration`: Build configuration (default: Release)
- `-BackendPath`: Path to backend folder (default: backend)
- `-OutputPath`: Output directory (default: build-output)
- `-SkipTests`: Skip running backend tests
- `-RunMigration`: Run database migrations after build
- `-ConnectionString`: Connection string for database migration
- `-VerboseOutput`: Show detailed build output
- `-PublishApps`: Publish and package Azure Function Apps (MainAPI, FileTransferAPI)

**What it does:**
- Cleans previous build and coverage artifacts
- Restores, builds, and (optionally) tests all backend projects
- Generates code coverage reports (HTML, Cobertura, badges)
- Publishes Function Apps and creates deployment zip files (if `-PublishApps` is set)
- Generates and copies database migration scripts
- Optionally runs database migrations if `-RunMigration` and `-ConnectionString` are provided

### 2. `build-and-test-ui-local.ps1`

Builds, tests, and packages the UI SPA.

**Usage:**
```powershell
./scripts/build-and-test-ui-local.ps1 [-OutputPath <path>] [-SkipTests] [-SkipE2E] [-VerboseOutput] [-PublishApps]
```

**Key Options:**
- `-OutputPath`: Output directory (default: build-output)
- `-SkipTests`: Skip running UI unit tests
- `-SkipE2E`: Skip running UI E2E (Playwright) tests
- `-VerboseOutput`: Show detailed build output
- `-PublishApps`: Create a deployment zip for the UI

**What it does:**
- Cleans previous build and coverage artifacts
- Installs npm dependencies
- Lints, builds, and tests the UI
- Generates code coverage reports (HTML, badges)
- Runs E2E tests with Playwright (unless `-SkipE2E` is set)
- Packages the UI build for deployment (if `-PublishApps` is set)

**Note:** UI E2E tests currently only work locally and are not stable in CI/CD.

### 3. `build-and-test-local.ps1`

All-in-one script: builds, tests, and packages both backend and UI.

**Usage:**
```powershell
./scripts/build-and-test-local.ps1 [-Configuration Release|Debug] [-OutputPath <path>] [-SkipTests] [-SkipUI] [-RunMigration] [-ConnectionString <string>] [-VerboseOutput] [-PublishApps] [-PublishUI]
```

**Key Options:**
- `-Configuration`: Build configuration (default: Release)
- `-OutputPath`: Output directory (default: build-output)
- `-SkipTests`: Skip all tests (backend and UI)
- `-SkipUI`: Skip UI build and test
- `-RunMigration`: Run database migrations after build
- `-ConnectionString`: Connection string for database migration
- `-VerboseOutput`: Show detailed build output
- `-PublishApps`: Publish backend Function Apps
- `-PublishUI`: Package UI for deployment

**What it does:**
- Cleans previous build and coverage artifacts
- Builds and tests backend and UI (unless skipped)
- Generates code coverage reports for both backend and UI
- Publishes backend Function Apps and UI deployment package (if requested)
- Handles database migration scripts and (optionally) applies migrations 