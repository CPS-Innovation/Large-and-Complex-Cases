tunnel_name=$(devtunnel list --json | jq --raw-output '.tunnels[0].tunnelId')
token=$(devtunnel token $tunnel_name --scopes connect --json | jq --raw-output '.token')
export DEVTUNNEL_TOKEN=$token
echo "Exported global env variable DEVTUNNEL_TOKEN=$DEVTUNNEL_TOKEN"