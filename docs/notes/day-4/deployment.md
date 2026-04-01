# Day 4 Deployment Notes

## Local project
Path:
`C:\dev\cloud-architecture-lab\src\lab-observability-api`

## Azure target
- Subscription: `gio-architecture-lab`
- Resource Group: `rg-ai-lab-dev-eastus`
- Region: `East US`

## Resources created
- App Service Plan: `asp-ai-lab-dev-eastus`
- Web App: `app-ai-lab-api-dev-eastus`
- Application Insights: `appi-ai-lab-api-dev-eastus`

## Deployment flow
1. Create ASP.NET Core Web API locally
2. Add Application Insights SDK
3. Add logging and test endpoints
4. Create App Service Plan
5. Create Web App
6. Create Application Insights resource
7. Add connection string to App Service app settings
8. Publish app locally
9. Deploy zip package to Web App
10. Validate live endpoints

## Commands used
powershell

az login

az account set --subscription "gio-architecture-lab"

az appservice plan create --name asp-ai-lab-dev-eastus-gio --resource-group rg-ai-lab-dev-eastus --location eastus --sku F1 --is-linux

az webapp create --resource-group rg-ai-lab-dev-eastus --plan asp-ai-lab-dev-eastus-gio --name app-ai-lab-api-dev-eastus-gio --runtime "DOTNETCORE:8.0"

az monitor app-insights component create --app appi-ai-lab-api-dev-eastus-gio --location eastus --resource-group rg-ai-lab-dev-eastus --application-type web

az monitor app-insights component show --app appi-ai-lab-api-dev-eastus-gio --resource-group rg-ai-lab-dev-eastus --query connectionString --output tsv

az webapp config appsettings set --resource-group rg-ai-lab-dev-eastus --name app-ai-lab-api-dev-eastus-gio --settings ApplicationInsights__ConnectionString="InstrumentationKey=c08131fc-85f6-4f69-9835-5d1deab5b3ff;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=ca795d53-f364-48aa-ab60-502be75330c6"

dotnet publish -c Release -o .\publish

Compress-Archive -Path .\publish\* -DestinationPath .\publish.zip -Force

az webapp deploy --resource-group rg-ai-lab-dev-eastus --name app-ai-lab-api-dev-eastus-gio --src-path .\publish.zip --type zip

NOTE: az webapp deploy failed. Needed to use Kudu File Manager and copy files manually to Path: home/site/wwwroot/.
(https://app-ai-lab-api-dev-eastus-gio.scm.azurewebsites.net)