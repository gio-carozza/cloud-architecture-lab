# Day-005 — Monitoring and Logging Notes

## Purpose

Day-005 introduces external AI provider integration into the application.

This means monitoring now becomes more important because outbound provider calls add new failure and latency risks.

This document defines the first monitoring goals for the Day-005 AI Gateway.

---

## What should be observed

### 1. AI endpoint usage

Track when `/api/ai/chat` is invoked.

Questions answered:

- how often is the endpoint used
- when was it last called
- which calls succeeded or failed

### 2. Provider call success and failure

Track whether requests to Anthropic succeed or fail.

Questions answered:

- how often are outbound calls failing
- what status codes are returned
- whether failures are caused by auth, billing, request shape, timeout, or service issues

### 3. Response latency

Track how long outbound provider requests take.

Questions answered:

- whether the model call is slow
- whether latency gets worse over time
- what timeout strategy may be needed later

### 4. Application health

Track whether the API itself remains healthy after AI integration.

Questions answered:

- whether `/health` still responds
- whether the AI integration broke normal hosting behavior
- whether deployment changes affect baseline app reliability

---

## Logging guidance for Day-005

At minimum, log:

- when AI endpoint is invoked
- which provider is used
- which model is configured
- whether provider call succeeded or failed
- status code on failure
- exception details when relevant

Do not log:

- raw secrets
- private API keys
- unsafe sensitive request content

---

## Suggested structured log fields

Recommended fields for future structured logging:

- `provider`
- `model`
- `endpoint`
- `statusCode`
- `durationMs`
- `requestId`
- `environment`

These fields will become more important in Day-006 and beyond.

---

## Initial Day-005 KQL ideas

These sample KQL queries are placeholders for when Application Insights / Log Analytics is expanded further.

### Recent requests

```kusto
requests
| where timestamp > ago(30m)
| order by timestamp desc