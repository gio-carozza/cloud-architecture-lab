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

## How to Publish to Azure

1. Go to the project directory
    - cd C:\dev\cloud-architecture-lab\src\lab-observability-api

2. Login into Azure
    - az login
    - az account set --subscription "gio-architecture-lab"

3. Publish app locally into a folder
    - dotnet publish .\lab-observability-api.csproj -c Release -o .\publish

4. Create a zip package from the published output
    - Compress-Archive -Path .\publish\* -DestinationPath .\lab-observability-api.zip -Force

5. Configure App Service to run from the deployed package
    - az webapp config appsettings set --resource-group "rg-ai-lab-dev-eastus" --name "app-ai-lab-api-dev-eastus-gio" --settings WEBSITE_RUN_FROM_PACKAGE=1

6. Get an Azure access token for Kudu publish authentication
    - $token = az account get-access-token --query accessToken -o tsv

7. Deploy the ZIP package directly to Kudu using the publish API
    - Invoke-RestMethod -Uri "https://app-ai-lab-api-dev-eastus-gio.scm.azurewebsites.net/api/publish?type=zip" -Method Post -Headers @{ Authorization = "Bearer $token" } -InFile ".\lab-observability-api.zip" -ContentType "application/zip"

NOTE: az webapp deploy failed. Needed to use Kudu File Manager and copy files manually to Path: home/site/wwwroot/.
(https://app-ai-lab-api-dev-eastus-gio.scm.azurewebsites.net)