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
For a complete, step-by-step guide to setting up your local environment—including environment variables, database setup, and troubleshooting—see the [Local Development Guide](doc/local-development.md).

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

- **Backend Build & Test:** [`backend-build-pipeline.yml`](devops-pipelines/deployments/application/build/backend-build-pipeline.yml)
  - Restores, builds, and tests all backend projects
  - Publishes code coverage and build artifacts
  - Packages and publishes Main API and FileTransfer API for deployment
- **UI Build & Test:** [`ui-build-pipeline.yml`](devops-pipelines/deployments/application/build/ui-build-pipeline.yml)
  - Installs dependencies, lints, tests, and builds the React SPA
  - Publishes code coverage and build artifacts
  - Optionally runs E2E tests with Playwright
- **UI Deploy:** [`ui-deploy-pipeline.yml`](devops-pipelines/deployments/application/ui-deploy-pipeline.yml)
  - Deploys the UI build artifact to the target Azure App Service instance
- **Backend Deploy:** [`backend-deploy-pipeline.yml`](devops-pipelines/deployments/application/backend-deploy-pipeline.yml)
  - Deploys backend build artifacts to Azure Function Apps and manages database migration scripts

> **Note:** The current deployment pipelines (`backend-deploy-pipeline.yml` and `ui-deploy-pipeline.yml`) are temporary solutions. Deployment automation and infrastructure provisioning are actively being improved and will be finalized using Terraform and more robust processes. Expect changes to these pipelines as the deployment approach is stabilized.

## Build and Test

- Backend and UI projects include unit and integration tests.
- Code coverage reports are generated and published as build artifacts.
- E2E tests for the UI are run using Playwright.
- **Note:** UI E2E tests currently only work in local development and are not yet stable in CI/CD pipelines.
- See the [Local Development Guide](doc/local-development.md) and pipeline YAMLs for details on running and customizing tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Security

If you discover a vulnerability or have a security concern, please refer to our [Security Policy](SECURITY.md) and contact Digital.Security@cps.gov.uk.

