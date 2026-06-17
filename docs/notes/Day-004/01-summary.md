# Day 4 Summary

## Objective

Move from architecture thinking into a first deployed Azure workload with basic observability.

## What I built

A minimal ASP.NET Core Web API called `lab-observability-api` and deployed it to Azure App Service.

## Azure resources used

- Resource Group: rg-ai-lab-dev-eastus-gio
- App Service Plan: asp-ai-lab-dev-eastus-gio
- Web App: app-ai-lab-api-dev-eastus-gio
- Application Insights: appi-ai-lab-api-dev-eastus-gio

## Endpoints implemented

- /
- `/health`
- `/api/test/ping`
- `/api/test/error`

## What I validated

- Local build and run worked
- Azure deployment worked
- Application Insights captured telemetry
- Logs and exceptions were visible after deployment

## What I learned

- A workload is not complete when it runs; it is complete when it can be observed
- Logging and telemetry should be part of the initial design
- App Service is a fast way to get an API online without managing infrastructure
- Operational Excellence becomes real when deployment, logging, and monitoring are connected

## Architect takeaway

Day 4 was the first shift from theory into an operating workload. The key lesson is that deployment is only one part of architecture. Visibility, failure analysis, and repeatability are equally important.
