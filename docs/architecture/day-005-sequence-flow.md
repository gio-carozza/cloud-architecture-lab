# Day 005 — Sequence Flow for AI Gateway v1

## Overview

This document describes the Day 005 request flow for the first AI-enabled version of the application.

The purpose of this flow is to show how the controller, service abstraction, provider implementation, and external model API interact.

---

## Primary flow

### Step 1 — Client sends request
The client sends an HTTP POST request to:

`/api/ai/chat`

Example request body:

```json
{
  "prompt": "Explain why deployment slots matter in Azure App Service."
}