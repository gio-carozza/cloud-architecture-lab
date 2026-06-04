# Day 8 — Architect Thinking

## The central decision: parallel seam, not extended seam

The first instinct is to bolt `SubmitBatchAsync` onto `IChatModelProvider`. It's one
provider, it feels like the same thing. The reason to resist is not style — it's
substitutability. Liskov: any `IChatModelProvider` that can't batch would need to throw
`NotSupportedException` on the batch methods. That's a broken contract, not a gap. ISP:
`AiController` would then depend on three methods it never calls. Those aren't
theoretical costs. They're the kind of costs that accumulate until you have a 400-line
interface and nobody remembers why half the methods are there.

`IBatchChatModelProvider` is a sibling, not an override. The interactive path is unchanged.
The batch path gets a contract shaped to what it actually does.

## What the ADR-009 symmetry proves

ADR-009 put caching inside the seam. ADR-010 takes batch outside. The fact that the same
test produced opposite answers is not inconsistency — it's the test working as designed.

The test: does this change alter the operation set or break substitutability?
- Caching: no new operations, `SendAsync` returns the same `ChatResponse`, a
  non-caching provider is still fully substitutable. Inside the seam.
- Batch: three new operations, no `ChatResponse` returned from submit (a handle instead),
  a non-batch provider cannot implement the methods without lying. Outside the seam.

Naming the test explicitly before writing the interface is the move. Without it you're
pattern-matching ("feels like the same provider") instead of reasoning.

## Why `ChatRequest` is reused without modification

Reusing `ChatRequest` as the element type of a batch is not the same as extending
`ChatRequest` with batch semantics. The distinction matters:

- Reusing it: a request authored for the interactive path can be submitted to batch
  unchanged. The contract is not modified. Clean.
- Extending it with a mode flag or nullable job ID: `ChatRequest` now means two
  things depending on runtime state. Every consumer branches on it. `ChatResponse`
  could return either a completion or a handle. The type's invariant is gone.

The right shape: `IReadOnlyList<ChatRequest>` as the batch input. `ChatRequest` doesn't
know it's in a batch. Neither does `ChatResponse`. The batch layer composes them.

## No resilience pipeline on submit — and why this matters in billing

The interactive resilience pipeline has an attempt timeout, a circuit breaker, and
a deliberately disabled retry. The batch submit must not have any retry — not even
a disabled one, because the next maintainer will re-enable it.

The failure mode: a network reset during `POST /v1/messages/batches` may have succeeded
server-side. The Anthropic API accepted the job, the acknowledgment was lost in transit.
Retrying submits a duplicate job. That duplicate runs, completes, and bills — and you
won't know until you see two job IDs in your account. Unlike most duplicate-submit
problems, this one has a direct cost impact at 50% of the synchronous rate.

The correct design: surface the error, let the caller decide whether to resubmit.
The gateway is stateless — it does not track whether a submit succeeded. That's the
caller's job. Explicitly removing the resilience pipeline from `ClaudeBatchApiClient`'s
`HttpClient` registration makes this policy visible in the code.

## The stateless gateway principle under batch

The interactive path is stateless: the gateway doesn't remember past requests. The batch
path preserves that: the gateway doesn't store batch IDs. The caller gets the ID on
submit and is responsible for polling. This is not a limitation — it's the right boundary.

The alternative (storing batch IDs in memory or a database) adds statefulness to a
stateless service. If the gateway restarts, in-flight IDs are gone. The "fix" requires
a persistence layer, a polling scheduler, and retry of failed results — none of which
belong in Day 8's scope, where the first real workload hasn't even been defined yet.
Building the orchestration layer before knowing the workload's requirements is the
trap ADR-010 explicitly names as Alternative 4 and rejects.

## What 50% savings looks like as a design principle

The savings are unconditional. No warm cache required. No minimum token threshold.
No hit-rate variance. Submit a batch, retrieve the results: every token costs half.
This is architecturally different from prompt caching, where the savings depend on
system prompt size, TTL, and request volume. Batch savings can be stated as a fact in
telemetry, not a probability. That makes the FinOps case straightforward to present.

The `EstimatedSavingsUsd` log line on retrieval is not cosmetic. It gives the team a
number without opening a billing dashboard. When the AI quality engineer runs the
regression suite, the log tells them what the run cost and what it would have cost
synchronously. That's the kind of observability that makes cost reduction visible
rather than assumed.

## Common beginner mistakes — observed in real systems

**Extending `IChatModelProvider`:** Happens when the architect thinks "same provider,
same interface." Works until the second provider arrives and someone has to implement
`SubmitBatchAsync` for a service that doesn't support batch. `NotSupportedException`
at runtime, not compile time.

**Retrying the batch submit:** Usually introduced during a "let's add resilience" pass.
The maintainer sees the batch client without a retry and adds one. Two months later,
billing shows double the expected cost for a specific night of batch runs where the
network was flaky. Hard to trace.

**Storing batch IDs in the gateway:** Feels helpful ("so callers don't lose the ID").
Requires session affinity when running multiple instances. Falls apart in a blue/green
deploy. The ID belongs to the caller; the gateway proxies, it doesn't own.

**Not surfacing savings as telemetry:** The batch path works, the cost goes down,
nobody knows by how much. The FinOps case is made in a quarterly review using
estimated numbers instead of measured ones. Observable systems earn more investment
than assumed ones.
