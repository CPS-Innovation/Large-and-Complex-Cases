tunnel_name=$(devtunnel list --json | jq --raw-output '.tunnels[0].tunnelId')
devtunnel token $tunnel_name --scopes connect --json
