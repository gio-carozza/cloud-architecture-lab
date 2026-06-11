# Day 6 — Architect Thinking

## 1. The OTel-vs-classic-AI-SDK fork: why ADR-008 exists

The original Day 6 plan called for `Serilog.Sinks.ApplicationInsights` and
`Microsoft.ApplicationInsights.AspNetCore`. Those packages make sense when you're
starting from nothing. Day 5 didn't start from nothing — it shipped
OpenTelemetry-first instrumentation via `Azure.Monitor.OpenTelemetry.AspNetCore`.

Installing both stacks on the same App Insights resource would have produced two
parallel telemetry pipelines emitting duplicate signals. Traces would have appeared
twice. Metrics would have been double-counted. Correlation across the two pipelines
would have been broken — each pipeline generates its own operation IDs and context
propagation headers. The result would have been expensive, confusing, and wrong.

The trap was caught before a single package was installed. The right response was
not to supersede ADR-006 — that decision (OTel as sole export pipeline) was
correct. The right response was to write ADR-008 as a refinement: Serilog stays
as a logging library inside the OTel pipeline, not parallel to it. Serilog formats
events for Console; the OTel `ILoggerProvider` captures the host's `ILogger` output
and exports to Azure Monitor. One signal source, one export path.

The pattern to remember: refinement ADRs are for when an original decision is sound
but a downstream implementation detail needs explicit treatment. Supersession ADRs
are for when the original decision was wrong. Get the category right or you'll spend
time arguing about the wrong thing in review.

## 2. Where Activity spans belong: tag where the data lives

The inner span (`claude.chat.api`) in `ClaudeApiClient` carries `llm.tokens.input`,
`llm.tokens.output`, and `llm.latency_ms`. The outer span (`ai.chat.complete`) in
`ClaudeChatModelProvider` carries `llm.provider` and `llm.model` only.

The alternative was to surface token counts on the outer span, since that's the
span a caller querying App Insights would look at first. Rejected. Token counts come
from the Anthropic response payload, which only `ClaudeApiClient` sees. To surface
them on the outer span, the provider would need to either re-parse data it already
discards or change the return type of `SendChatAsync` to carry usage metadata. Both
options couple layers that don't need to be coupled.

The rule: tag where the data naturally lives. The transport layer owns transport
data — HTTP status, latency, token counts from the response body. The orchestration
layer owns orchestration data — which provider, which model, which abstraction was
invoked. Nested spans give the operational story: "where in the request did time
go?" The outer span answers "was this an AI chat call?" The inner span answers "how
did the Anthropic HTTP round-trip perform?"

When a second provider is added (Azure OpenAI, Bedrock), each will have its own
inner span with its own transport tags. The outer span stays identical. That's the
provider abstraction paying dividends in observability — the same KQL query works
regardless of which provider handled the request.

## 3. Why no retries on chat-generation POST

Chat generation is a paid, non-idempotent POST. Retrying on failure duplicates
cost with no guarantee the second call produces a coherent result in the context of
a streaming conversation. It also creates confusing user-facing behavior — the
caller may have already surfaced an error and retrying silently could produce a
second response the caller never asked for.

The right resilience shape for LLM chat is: timeout + circuit breaker, no retries.
Timeout protects the caller from a slow provider. Circuit breaker protects the
provider from being hammered when it's struggling. Retries protect against transient
failures — but the question "is this transient?" requires classifying the error,
which Day 6 doesn't do yet.

Day 6 expressed the no-retry intent as `MaxRetryAttempts = 0`.
`Microsoft.Extensions.Http.Resilience` v10 rejects 0 — the validator requires at
least 1. The fix: `MaxRetryAttempts = 1` with `ShouldHandle = _ => false`. The
retry stage exists in the pipeline but the predicate never fires. The Polly log
confirms this: `Handled: 'False', Attempt: '0'`.

Day 7 will replace the `ShouldHandle` stub with a classification-based predicate:
transient errors (429, 503, 504) retried once with jitter; auth and billing errors
(401, 403, 400) not retried. That's the moment the retry stage earns its place in
the pipeline.

## 4. Token cost as a first-class metric, not a log field

Token counts land on Activity tags, not in Serilog log lines. This is not an
aesthetic preference.

Metrics aggregate cheaply — App Insights can compute `sum(llm.tokens.input)` per
hour across millions of requests with a single KQL aggregation. Log-based token
tracking requires ingesting every log line that contains a token count, then
aggregating over that corpus. At low volume it doesn't matter. At enterprise volume
it's an ingestion and query cost problem you created for yourself.

The answer to "what's our token cost per hour?" should be one KQL query against a
metric table, not a log scan. That question will be asked in every budget review
and every incident post-mortem. The observability design should make it trivially
answerable.

The same logic applies to provider latency. `llm.latency_ms` on the Activity tag
feeds latency percentile aggregations in App Insights. The equivalent log field
(`DurationMs` in the structured log line) feeds the request log, which is useful
for per-request debugging but not for trend analysis. Both are present. They serve
different purposes. Neither replaces the other.

## 5. The validate-on-start lesson

The compiler caught nothing wrong with `SamplingDuration = 30s, AttemptTimeout = 45s`.
The types are correct. The values are valid integers. The program compiled cleanly.

The startup validator caught the semantic violation: sampling duration must be at
least double the attempt timeout, or the circuit breaker observation window is too
short to be statistically meaningful. The resilience library enforces this
invariant at startup, not at compile time, because it's a mathematical relationship
between two configuration values — not something a type system can express.

The lesson generalizes: any options class bound to a library with semantic
constraints should wire `ValidateOnStart`. The class of errors `ValidateOnStart`
catches is specifically the class the compiler cannot — cross-field invariants,
range constraints, and mathematical relationships between configuration values.
`ValidateOnStart` is the compiler's extension into configuration space.

The graveyard has two entries from Day 6 that validate this point. See
`docs/standards/_principles.md`.

## 6. The intent-vs-encoding distinction

ADR-006 was written on April 15. It says: no retries on chat POST. That intent is
unchanged today.

The code that expresses that intent changed. Originally `MaxRetryAttempts = 0`;
now `MaxRetryAttempts = 1` with `ShouldHandle = _ => false`. Same architectural
decision, different encoding. The encoding changed because
`Microsoft.Extensions.Http.Resilience` v10 tightened its validation rules.

The temptation when a library change forces a code change is to update the ADR to
match. Resist it. The ADR captures *why* — the non-idempotent cost argument, the
classification-deferred rationale. The code captures *how* — which knobs were
turned to express that intent within the current library's constraints. They're
allowed to drift because they answer different questions.

The corollary: when an ADR and its code implementation are obviously diverged, the
right question is not "which one is wrong?" but "is the intent still correct?" If
yes, update the code. If no, write a supersession ADR. Day 6 updated the code.
The intent stood.

Over 11 months of this roadmap, this pattern will repeat. Library versions change.
API shapes shift. Cloud services get new defaults. The ADR corpus is the stable
artifact — it accumulates the reasoning. The code expresses the current best
encoding of that reasoning. Keep them coherent, but don't conflate them.
