#!/bin/bash

if [ "$APP_TYPE" != "functionapp" ] && [ "$APP_TYPE" != "webapp" ]; then
    echo "##[error]❌ The value '$APP_TYPE' is invalid input for \$APP_TYPE. The value must be either 'functionapp' or 'webapp'."
    exit 1
fi

echo "DEBUG: use_slot = $USE_SLOT"
APP_STATE_CMD="az $APP_TYPE show --name $APP_NAME --resource-group $RG --query "state" -o tsv"
echo "DEBUG: app_state_cmd = $APP_STATE_CMD"

if [ "$USE_SLOT" = "true" ]; then
    APP_STATE_CMD+=" --slot $SLOT_NAME"
    INSTANCE_NAME="$APP_NAME-$SLOT_NAME"
    echo "DEBUG: We've entered the first part of the IF block."
    echo "DEBUG: app_state_cmd = $APP_STATE_CMD"
    echo "DEBUG: instance_name = $INSTANCE_NAME"
else
    INSTANCE_NAME="$APP_NAME"
    echo "DEBUG: We've entered the second part of the IF block."
    echo "DEBUG: instance_name = $INSTANCE_NAME"
fi

echo "DEBUG: After IF block: app_state_cmd = $APP_STATE_CMD"

check_app_state() {
    local max_retries=3
    local attempt=1
    local sleep=5

    while [ $attempt -le $max_retries ]; do

    echo "DEBUG: We've entered the function's while loop."
    echo "DEBUG: app_state_cmd = $APP_STATE_CMD"
    echo "DEBUG: instance_name = $INSTANCE_NAME"

    echo "Attempt $attempt/$max_retries: Waiting $sleep seconds for the deployment to stabilize..."
    sleep $sleep

    echo "Running command: "$APP_STATE_CMD""
    APP_STATE=$(eval "$APP_STATE_CMD" 2>&1)
    EXIT_CODE=$?

    if [ $EXIT_CODE -ne 0 ]; then
        echo "##[error]❌ The command failed. $APP_STATE"
        exit 1
    fi

    echo "The app state: $APP_STATE"

    if [ "$APP_STATE" = "Running" ]; then
        echo "✅ The app is in a Running state."
        echo "##vso[task.setvariable variable=instanceName;]$INSTANCE_NAME"
        return 0
    else
        echo "App state is: "$APP_STATE""
        echo "##[warning]⚠️ The app is not in Running state."
    fi

    attempt=$((attempt + 1))
    sleep=$((sleep + 5))
    done

    echo "❌ The app did not reach 'Running' state after $max_retries attempts."
    exit 1
}

echo "Verifying deployment to $INSTANCE_NAME..."
check_app_state