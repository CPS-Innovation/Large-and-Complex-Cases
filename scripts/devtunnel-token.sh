tunnel_name=$(devtunnel list --json | jq --raw-output '.tunnels[0].tunnelId')
token=$(devtunnel token $tunnel_name --scopes connect --json | jq --raw-output '.token')
echo
echo
echo "Token for today:"
echo
echo $token