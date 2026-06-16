# API Versioning Standard

**Phase:** lower priority — enforce before Phase 2 goes external
**Applies to:** all HTTP endpoints in `src/lab-observability-api/Controllers/`

---

## Versioning strategy

URL-path versioning. Version prefix is `/api/v{N}/`:

```text
/api/v1/ai/chat          ← current (implicit v1 — no prefix until a v2 exists)
/api/v2/ai/chat          ← added when v1 is deprecated but not yet removed
/api/v1/ai/chat/stream
/api/v1/ai/batch
/api/v1/health
```

For Phase 1: no version prefix is required while a single version exists. Add the `v1` prefix only when a `v2` is being introduced. Retrofitting the prefix is a non-breaking change for internal callers.

Do NOT use header-based versioning (`Api-Version: 2`) — it is invisible in browser dev tools, harder to test, and harder to document in Swagger.

---

## Breaking vs. non-breaking changes

### Non-breaking (no version bump required)

- Adding an optional request field with a safe default
- Adding a new response field
- Adding a new endpoint
- Changing error message text (not error codes)
- Performance improvements with identical contract

### Breaking (requires new version)

- Removing a request or response field
- Renaming a request or response field
- Changing a field's type
- Changing an endpoint's HTTP method
- Removing an endpoint
- Changing an error code meaning

---

## `ChatRequest` / `ChatResponse` / `ChatChunk` evolution rules

These are the provider-agnostic contracts. Breaking changes to them affect every provider and every caller simultaneously.

| Change type | Allowed without ADR? | Notes |
|---|---|---|
| Add optional field | Yes | Must have a safe default; document in ADR Implementation Notes |
| Add required field | No — ADR required | Breaking for existing callers; requires version bump |
| Remove any field | No — ADR required | Always breaking |
| Rename any field | No — ADR required | Always breaking |
| Change field type | No — ADR required | Always breaking |

When a breaking change is unavoidable: create a new ADR, introduce the new contract version, run both versions in parallel for at least one full day of testing before removing the old version.

---

## Deprecation policy

1. Mark the old endpoint or field as `[Obsolete]` in C# with a descriptive message.
2. Add a Swagger annotation: `[ApiExplorerSettings(IgnoreApi = false)]` + deprecation note.
3. Log a `Warning` on every call to the deprecated path: `"Deprecated endpoint called: {Path}. Migrate to {NewPath}."`
4. Keep the deprecated version running for a minimum of 14 days after the new version is available.
5. Remove only after confirming zero traffic to the deprecated path (KQL: zero `requests` for the old route over the last 7 days).

---

## Swagger / OpenAPI

All endpoints must have Swagger annotations in Phase 2 (external callers need a contract). Minimum per endpoint:

```csharp
/// <summary>Send a chat prompt to the AI gateway.</summary>
/// <response code="200">Successful completion</response>
/// <response code="400">Validation failed</response>
/// <response code="503">Provider unavailable</response>
[ProducesResponseType(typeof(ChatResponse), 200)]
[ProducesResponseType(typeof(ApiError), 400)]
[ProducesResponseType(typeof(ApiError), 503)]
```

The streaming endpoint (`/chat/stream`) must document the SSE event format and the `event: error` frame in its Swagger description.

---

## Versioning in ADRs

Every ADR that introduces a breaking contract change must include a "Migration path" section:

- What callers must change
- What the parallel-run window is
- What signals confirm it is safe to remove the old version
