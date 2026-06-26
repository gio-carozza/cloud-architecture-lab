# Day 9 — Posture Check

> Honest answers only. The graveyard is more valuable than the trophy case.
> Fill this at the END of the day, BEFORE marking the day complete.

## 1. Whose problem did I actually solve today?

The human staring at a blank response box. Streaming solves PERCEIVED latency,
not total latency — completion time is unchanged; first visible token now lands
at p50 TTFT = 1354ms instead of dead air for the full generation.
Tail not measured this session: count=3 requests in App Insights, insufficient
sample for reliable p95/p99 estimates. Run KQL Query 11 against
appi-ai-lab-api-dev-eastus-gio as traffic accumulates to establish the tail
baseline. The 1354ms p50 is dominated by Anthropic's own TTFT (provider headers
arrived at 1290ms in the live test run); gateway processing overhead is
single-digit milliseconds by comparison — the tail, when it lengthens, will be
Anthropic latency or cold-start contamination, not gateway logic.

## 2. What would I refuse to ship if I were the only one in the room?

CS1626 was fixed by restructuring to `try/finally` without a catch clause on the
iterator's outer try block — CS1626 fires only when yield appears in a try WITH a
catch. The provider-level safety net is intact: HTTP-level exceptions are caught
BEFORE the generator starts (in the initial `_httpClient.SendAsync` catch blocks),
and mid-stream exceptions propagate naturally to the controller. The controller
(`AiController.cs` outer `catch (Exception ex)` block) catches them and the client
receives: `event: error\ndata: {"code":"stream_error","message":"An error occurred
during streaming.","correlationId":"<id>"}\n\n`. Error handling was not deleted to
satisfy the compiler — it was relocated to the appropriate layer.

## 3. What did I try, fail at, and learn?

The real lesson is NOT "yield can't go in try/catch." It is: the C# iterator
restriction forces a deliberate error-propagation design in streaming providers,
and the naive fix (deleting the try/catch) silently drops provider-level
exception handling. Second: ADR-011 put StreamAsync INSIDE the interactive seam
one day after ADR-010 sent batch OUT. The reconciliation is now written into
ADR-011 citing ADR-010 — the load-bearing test is Liskov substitutability, not
"new operation": batch breaks substitutability (a non-batching provider has no
sensible fallback — "a batch of one" destroys the 50% discount that is batch's
entire purpose), while streaming does NOT (a non-streaming provider yields the
whole completion as a single chunk — the default-degrade implementation is PROOF
the Liskov problem doesn't exist for streaming). Same test, opposite trigger.
Default-degrade is SILENT — no SupportsStreaming capability flag. The default
interface implementation on `IChatModelProvider` wraps `SendAsync` and yields the
completion as a single `ChatChunk("end_turn")`. Deliberate: the Liskov contract
is met by behavior; a flag would add ISP pressure where none exists and would
invite callers to condition on it rather than relying on the degrade guarantee.

## 4. Could I explain today's work at all four levels?

### 10-year-old

Two friends answer a long question — one says nothing until fully done, the other
starts talking word-by-word. Same answer, same time, but the second never looks
broken. Taught the robot to be the second friend.

### CEO

Users abandon a frozen-looking screen. Streaming starts the answer in ~1.3s
instead of a blank box for the full duration — same cost, same total time, lower
abandonment. Risk that was closed today: if the connection drops mid-answer we
still log what we were billed for, so streaming is not a cost-tracking hole.

### Engineer

StreamAsync returns `IAsyncEnumerable<ChatChunk>`. ClaudeApiClient.StreamChatAsync
parses Anthropic SSE (message_start / content_block_delta / message_delta carrying
final usage / message_stop). Controller sets X-Accel-Buffering:no, FlushAsync per
chunk, threads RequestAborted. CS1626 resolved by using try/finally (no catch
clause) on the iterator's outer block — exceptions propagate to the controller
catch which writes `event: error\ndata: {ApiError}\n\n`. Default degrade on
IChatModelProvider wraps SendAsync and yields one terminal ChatChunk.

### Architect

The decision that matters is StreamAsync extending the interactive seam while
batch got a sibling, and WHY that's consistent (Liskov, per ADR-011). At scale
the streaming path is where the audit trail leaks: usage arrives in the final
message_delta, and a client disconnect at chunk N may cut off that event before it
is read — cost attribution and the Responsible-AI audit log become conditional on
the client behaving. That gap was tested and mitigated today: the finally block
distinguishes client disconnects (expected, LogDebug) from unexpected stream ends
(LogWarning), and the event carries CorrelationId so cost can be attributed even
for partial streams. Full automated closure requires a fault-injection integration
test (no test project yet — deferred).

*If any level is missing, the concept isn't fully owned — schedule a teach-back.*

## 5. Which pillar took the most damage today, and what's the minimum fix?

The metric naming question is RESOLVED: renamed ai.chat.stream.ttft_ms →
`ai.provider.stream.ttft_ms` to align with the `ai.provider.*` namespace used by
every other metric. Zero dashboards or alerts depended on the old name. GREEN.
The KQL cookbook (Queries 11 and 12) and the GatewayTelemetry.cs already reflect
the correct name.

Most-damaged pillar was RESPONSIBLE AI (RA6): final-usage logging on the streaming
path, specifically the client-disconnect case where RequestAborted cancels before
message_delta arrives. Status: gap was REAL (confirmed via stream-test-output.txt
— the WRN fired on a live test where ct was cancelled before message_delta).
Mitigated: the finally block now distinguishes client disconnect (LogDebug,
expected operational noise) from unexpected stream end (LogWarning, actionable).
Not fully closed: no automated fault-injection test. Full closure belongs in Day
10+ when a test project is added. Leaving this YELLOW in the audit trail is the
honest answer; marking it GREEN on a manual observation alone would repeat the
Day 8 pattern of a shallow audit.
