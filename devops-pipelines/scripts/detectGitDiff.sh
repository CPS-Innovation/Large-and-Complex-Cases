#!/bin/bash
set -e

if [ -z "$PATH_TO_CHECK" ]; then
  echo "PATH_TO_CHECK is not set"
  exit 1
fi

BASE_BRANCH=${BASE_BRANCH:-main}

OUTPUT_VAR_NAME=${OUTPUT_VAR_NAME:-isDiffDetected}

echo "Checking for git diff in path: $PATH_TO_CHECK"
echo "This script will set an output variable named: $OUTPUT_VAR_NAME"

# Ensure sufficient history
git fetch origin $BASE_BRANCH --depth=2

if [ -n "$SYSTEM_PULLREQUEST_TARGETBRANCH" ]; then
  echo "PR pipeline detected"

  TARGET_BRANCH=${SYSTEM_PULLREQUEST_TARGETBRANCH#refs/heads/}
  git fetch origin "$TARGET_BRANCH" --depth=1

  DIFF_RANGE="origin/$TARGET_BRANCH...HEAD"
else
  echo "Merge pipeline detected"

  if git rev-parse HEAD~1 >/dev/null 2>&1; then
    DIFF_RANGE="HEAD~1...HEAD"
  else
    echo "Insufficient history to detect changes"
    echo "##vso[task.setvariable variable=$OUTPUT_VAR_NAME;isOutput=true]false"
    exit 0
  fi
fi

CHANGED_FILES=$(git diff --name-only $DIFF_RANGE -- "$PATH_TO_CHECK")

if [ -n "$CHANGED_FILES" ]; then
  echo "Changes detected:"
  echo "$CHANGED_FILES"
  echo "##vso[task.setvariable variable=$OUTPUT_VAR_NAME;isOutput=true]true"
else
  echo "No changes detected in $PATH_TO_CHECK"
  echo "##vso[task.setvariable variable=$OUTPUT_VAR_NAME;isOutput=true]false"
fi