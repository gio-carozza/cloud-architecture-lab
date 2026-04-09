# Day 005 — App Settings Template

## Purpose

This document defines the configuration values required for Day 005 AI Gateway v1.

These settings should be supplied through environment-specific configuration.

For Azure deployment, these values should be configured in:

- Azure App Service
- Settings
- Environment variables

No real secret should be committed into source control.

---

## Required settings

### Anthropic__ApiKey
The Anthropic API key used by the application to authenticate outbound requests.

Example:

```text
Anthropic__ApiKey = sk-ant-xxxxxxxxxxxxxxxx