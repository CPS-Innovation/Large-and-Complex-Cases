# Large and Complex Cases (LACC)

## Introduction
The Large and Complex Cases (LACC) project aims to deliver a secure, auditable, and performant solution for managing complex cases. The system is designed to increase user productivity by centralising access to case material (including correspondence), providing powerful search capabilities, and integrating with other systems of record to reduce data re-keying.

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v20+)](https://nodejs.org/)
- [npm](https://www.npmjs.com/)
- PowerShell (for local scripts)
- PostgreSQL (see [Local Development Guide](doc/local-development.md) for setup details)
- Azure Functions Core Tools (see [Local Development Guide](doc/local-development.md))

### Local Development
For a complete, step-by-step guide to setting up your local environment—including environment variables, database setup, and troubleshooting — see the [Local Development Guide](doc/local-development.md).

To build, test, and package the backend and UI locally, use the provided PowerShell scripts in the `scripts/` directory:

- `build-and-test-backend-local.ps1`: Build, test, and package backend services.
- `build-and-test-ui-local.ps1`: Build, test, and package the UI SPA.
- `build-and-test-local.ps1`: Build, test, and package both backend and UI together.

Each script provides options for running tests, generating coverage reports, and producing deployment-ready artifacts. See the [Local Development Guide](doc/local-development.md) for script usage, options, and troubleshooting.

**Architecture Note:**
- The UI communicates only with the **Main backend API** (`CPS.ComplexCases.API`), which acts as a gateway for all frontend requests.
- The Main API orchestrates business logic and integrates with other backend services, including the **FileTransfer API** (`CPS.ComplexCases.FileTransfer.API`).
- The Main API uses a dedicated HTTP client to call the FileTransfer API for all file transfer operations (upload, list, status, etc.).
- **Both the Main API and FileTransfer API must be running locally for the application to function end-to-end.** If the FileTransfer API is not running, any file transfer features in the UI or Main API will not work.

### CI/CD Pipelines
The project uses Azure Pipelines for automated build, test, and deployment. Key pipeline definitions:

#### Backend
The following are triggered in response to any changes pushed to the [`backend`](backend) directory.

- **Backend Build & Test:** [`backend-pr-build-and-test.yml`](devops-pipelines/backend/backend-pr-build-and-test.yml)
  - Trigger: PR against main.
  - Restores, builds, and tests all backend projects.
  - Publishes test results and code coverage.
- **Backend Build & Publish:** [`backend-build-and-publish.yml`](devops-pipelines/backend/backend-build-and-publish.yml)
  - Trigger: Merge to main branch.
  - Packages and publishes Main API and FileTransfer API for deployment.
  - Generates and publishes database migration scripts for deployment.
- **Backend Deploy:** [`backend-deploy.yml`](devops-pipelines/backend/backend-deploy.yml)
  - Trigger: A successful run of the build and punlish pipeline above.
  - Runs the following stages in the dev environment:
    - Updates the Key Vault with configuration values for the function apps.
    - Runs the migration scripts against the database.
    - Deploys build artifacts to Azure Function Apps.
  - Pauses for manual validation before repeating the process in staging.

#### UI
The following are triggered in response to any changes pushed to the [`ui`](ui) directory.

- **UI Build & Test:** [`ui-pr-build-and-test.yml`](devops-pipelines/ui/ui-pr-build-and-test.yml)
  - Trigger: PR against main.
  - Installs dependencies, lints, builds and runs unit tests for the React SPA.
  - Runs E2E tests with Playwright.
  - Publishes code coverage and test results.
- **UI Build & Deploy:** [`ui-build-and-deploy.yml`](devops-pipelines/ui/ui-build-and-deploy.yml)
  - Trigger: Merge to Main branch.
  - Runs the following jobs for the dev environment:
    - Builds the SPA using environment-specific VITE variables.
    - Publishes the build artifact.
    - Deploys the build artifact to the target Azure App Service instance.
  - Pauses for manual validation before repeating the process for staging.


## Build and Test

- Backend and UI projects include unit and integration tests.
- Code coverage reports are generated and published as build artifacts.
- E2E tests for the UI are run using Playwright.
- See the [Local Development Guide](doc/local-development.md) and pipeline YAMLs for details on running and customizing tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Security

If you discover a vulnerability or have a security concern, please refer to our [Security Policy](SECURITY.md) and contact Digital.Security@cps.gov.uk.

