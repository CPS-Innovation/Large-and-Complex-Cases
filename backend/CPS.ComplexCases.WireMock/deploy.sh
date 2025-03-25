rm -rf publish
rm publish.zip
dotnet clean
dotnet publish -o publish -c Release
(cd publish && zip -r ../publish.zip *)  
az webapp deployment source config-zip --src publish.zip -n as-web-mock-ddei-temp -g rg-beta-temp