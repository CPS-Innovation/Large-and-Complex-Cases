# PR checklist

Tick what applies before you raise. Delete anything that doesn't.

## Correctness
- [ ] Does what the ticket asks for
- [ ] Handles the edge cases (empty, null, zero, large inputs), not just the happy path
- [ ] Errors are handled, not swallowed
- [ ] New logic has tests, including the failure cases
- [ ] Tests assert something real, not just run the code

## Keeping it simple
- [ ] Doesn't rebuild something that already exists in the codebase
- [ ] No more complex or slower than it needs to be
- [ ] No leftover debug logging or commented-out code

## Easy to miss (a green pipeline won't flag these)
- [ ] One logical change, not a pile of unrelated stuff

### If you touched the backend
- [ ] Schema changes have a migration, and the Down() doesn't drop anything it shouldn't
- [ ] New app settings are wired into the fa-config templates
- [ ] Secrets use Key Vault references, not plain text
- [ ] New outbound HTTP clients go through the shared resilience handler (AddResiliencePolicyHandler), rather than hand-rolling retries
- [ ] New calls to an external service (DDEI, Egress, NetApp) have client tests against a WireMock stub (for the serialisation, auth and error handling) and are covered by the integration tests that run against the real dev services
- [ ] Ran dotnet format, since CI builds and tests but doesn't check formatting

### If you touched the UI
- [ ] Ran the e2e/ suite locally (the PR build only runs the mocked tests; the full suite runs post-merge and gates the deploy)
- [ ] API changes are reflected in the MSW handlers in src/mocks and the matching zod schema, so the mocked tests aren't passing against a contract that's gone
- [ ] New config or feature flags are wired into the VITE build variables, not just added in code
- [ ] Data fetches handle the loading, empty and error states, not just success, and match how the rest of the app already does it
- [ ] Ran prettier (npm run prettier:format), since CI lints but doesn't check formatting
- [ ] Checked accessibility (keyboard nav, screen reader, sensible labels), since the Lighthouse run is manual and won't flag it for you