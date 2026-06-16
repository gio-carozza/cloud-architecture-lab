# Testing Standard

**Phase:** 1 (active now)
**Applies to:** `src/lab-observability-api/` and `src/lab-observability-api.Tests/`

---

## What must be tested

Every new feature or bug fix requires tests before the day closes. No exceptions.

| Layer | What to test | Test type |
|---|---|---|
| Controllers | Happy path, validation failures, cancellation | Integration (via `GatewayWebApplicationFactory`) |
| Provider implementations | Request mapping, response mapping, error translation | Unit (mock `HttpClient`) |
| Middleware | Correlation ID generation, propagation, header output | Integration |
| Options binding | Required fields present, defaults applied | Unit |
| Telemetry | Span tags set, counters incremented, histogram recorded | Unit (fake `ActivityListener`) |
| Provider abstraction contract | `ChatRequest`/`ChatResponse`/`ChatChunk` carry no provider types | Static analysis + unit |

## What NOT to test

- Anthropic API behavior (that's their SLA, not ours)
- Azure App Service behavior
- `HttpClient` internals
- Framework code (ASP.NET routing, DI container, Serilog sinks)

Never mock the provider seam in controller tests — use `GatewayWebApplicationFactory` with a fake provider that returns a controlled response.

---

## Unit vs. integration boundary

```text
Unit test:     one class, all dependencies mocked/faked, no I/O
Integration:   GatewayWebApplicationFactory + real DI + fake HTTP handler
E2E:           NOT in this repo — verified manually post-deploy
```

`GatewayWebApplicationFactory` replaces the real `HttpClient` with a `FakeHttpMessageHandler`. It does NOT call Anthropic. It does NOT require Azure.

---

## Test naming convention

```csharp
// Pattern: MethodName_StateUnderTest_ExpectedBehavior
[Fact]
public async Task SendAsync_WhenPromptIsEmpty_Returns400()

[Fact]
public async Task StreamAsync_WhenClientDisconnects_CancellationTokenPropagated()

[Theory]
[InlineData(null)]
[InlineData("")]
public async Task Chat_WhenPromptIsNullOrEmpty_ReturnsBadRequest(string prompt)
```

---

## Coverage floor

No hard line count. Instead: every public method on every class in `Services/AI/`, `Services/Claude/`, `Controllers/`, and `Middleware/` must have at least one test covering its primary path and one covering its primary failure path.

Run `dotnet test` before any commit. A red test is a build-blocker — do not commit with failing tests.

---

## Adding tests for each new feature

When a new file appears in `07-files-changed.md` under `src/`, the corresponding test must appear in the same row's `Change` column or in a separate row. If no test was written, the audit log must record it as a YELLOW debt item.

Checklist per new source file:

- [ ] Happy path test exists
- [ ] At least one error/edge case test exists
- [ ] `CancellationToken` propagation tested if the method is `async`
- [ ] Provider-agnostic contract verified (no Anthropic types in public model properties)
- [ ] New telemetry signals (spans, counters, histograms) have a test asserting they fire

---

## Fake and mock patterns

**Prefer fakes over mocks** for interfaces that are called multiple times or have state:

```csharp
// Good — fake with controlled behavior
public class FakeChatModelProvider : IChatModelProvider
{
    public ChatResponse NextResponse { get; set; } = new("fake", "fake-model", "OK");
    public Task<ChatResponse> SendAsync(ChatRequest r, CancellationToken ct)
        => Task.FromResult(NextResponse);
    public async IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest r,
        [EnumeratorCancellation] CancellationToken ct)
    { yield return new ChatChunk("OK", "end_turn", null); }
}
```

**Use `Moq` sparingly** — only for verifying a method was called with specific arguments. Never mock `HttpClient` directly; use `FakeHttpMessageHandler`.

---

## Regression tests

When a bug is fixed, a test reproducing the bug must be added before the fix is committed. The test name must include `Regression`:

```csharp
[Fact]
public async Task StreamAsync_Regression_CancellationTokenNotPropagatedToUpstreamRead()
```
