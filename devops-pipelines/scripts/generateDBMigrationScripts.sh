#!/bin/bash

set -euo pipefail

# Helper echo functions
info()   { echo -e "\033[1;34m[INFO]\033[0m  $*"; }
warn()   { echo -e "\033[1;33m[WARN]\033[0m  $*"; }
error()  { echo -e "\033[1;31m[ERROR]\033[0m $*"; }

# Ensure dotnet-ef 
export PATH="$PATH:$HOME/.dotnet/tools"

if ! dotnet ef --version &> /dev/null; then
  dotnet tool install --global dotnet-ef --version $EF_VERSION
fi

# Generate FORWARD (idempotent) script --------------------------------
info "Generating DB FORWARD migration script → ${MIGRATION_SCRIPT_OUT}"

if ! dotnet ef migrations script \
  --no-build \
  --idempotent \
  --output "${MIGRATION_SCRIPT_OUT}" \
  --project "${DATA_PROJECT_PATH}" \
  # --startup-project "${STARTUP_PROJECT_PATH}"
then
  error "Forward migration script generation FAILED."
  exit 1
fi

info "Forward migration script generated successfully."

# ---- 2) Generate ROLLBACK (idempotent) script -------------------------------

info "Generating DB ROLLBACK migration script → ${ROLLBACK_SCRIPT_OUT}"
info "[Step 1/2] Retrieving the last two migrations from source code..."

# Capture migrations list (no DB connection needed) and normalize CRLF to LF
# We filter lines that look like migration IDs: 14 digits + underscore
readarray -t migrations < <(
  dotnet ef migrations list \
    --no-build \
    --no-connect \
    --project "${DATA_PROJECT_PATH}" \
    --startup-project "${STARTUP_PROJECT_PATH}" \
  | tr -d '\r' \
  | grep -E '^[0-9]{14}_'
)

# Validate we have at least two migrations for a "latest → previous" rollback
if (( ${#migrations[@]} < 2 )); then
  warn "Found fewer than two migrations (count=${#migrations[@]}). Skipping rollback script generation."
  exit 0
fi

latest="${migrations[$((${#migrations[@]} - 1))]}"
previous="${migrations[$((${#migrations[@]} - 2))]}"

info "Latest migration:   ${latest}"
info "Previous migration: ${previous}"
info "[Step 2/2] Generating idempotent rollback (latest → previous)..."

# Temporarily allow command to fail without exiting the whole script
set +e
dotnet ef migrations script "${latest}" "${previous}" \
  --no-build \
  --idempotent \
  --output "${ROLLBACK_SCRIPT_OUT}" \
  --project "${DATA_PROJECT_PATH}" \
  --startup-project "${STARTUP_PROJECT_PATH}"
rollback_rc=$?
set -e

if [[ $rollback_rc -ne 0 ]]; then
  warn "Rollback script generation FAILED (exit code ${rollback_rc}). Proceeding without failing the job."
else
  info "Rollback script generated successfully."
fi

info "Done."
