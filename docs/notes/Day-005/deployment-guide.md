# Day-005 — Deployment Guide

## Purpose

This document records the working deployment path used to publish the Day-005 AI Gateway to Azure App Service.

This guide reflects the deployment approach that worked after transport issues were encountered with the earlier deployment path.

---

## Target application

- Subscription: `gio-architecture-lab`
- Resource Group: `rg-ai-lab-dev-eastus`
- App Service: `app-ai-lab-api-dev-eastus-gio`

---

## Working deployment sequence

### 1. Go to the project directory

```powershell
cd C:\dev\cloud-architecture-lab\src\lab-observability-api