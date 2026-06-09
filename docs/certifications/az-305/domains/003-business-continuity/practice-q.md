# Practice Questions — Design Business Continuity Solutions (AZ-305 Domain 3)

---

## Q1: Retry policy — error classification

**Scenario:** An AI gateway makes HTTP calls to an external LLM provider. The team implements a retry policy that retries on any non-2xx HTTP status. On a deployment with an expired API key, the gateway enters a retry loop and exhausts all retry attempts before returning an error, adding 15 seconds of delay to every request.

**Question:** What is the correct fix for the retry policy?

A) Reduce the number of retries from 5 to 2  
B) Increase the timeout to 60 seconds so retries have more time to succeed  
C) Classify 401 and 403 as non-retriable errors — authentication failures are permanent for that request and retrying cannot fix them  
D) Use exponential backoff — the 15-second delay proves retries are not fast enough  

**Answer:** C

**Why:** 401 (Unauthorized) and 403 (Forbidden) are client-side errors caused by invalid credentials or permissions — conditions that will not change between retries. The correct policy retries only on 429 (rate limit), 5xx (transient server errors), and network errors. A) reducing retry count lessens the symptom but doesn't fix the classification bug. B) longer timeouts make the symptom worse. D) the problem is retrying non-retriable errors, not retry speed.

**Exam domain:** Design business continuity solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q2: Circuit breaker — cascading failure prevention

**Scenario:** An e-commerce platform's product recommendation service calls an AI provider API. The AI provider experiences a 10-minute outage. The recommendation service keeps sending requests that hang for 30 seconds each before timing out. Product catalog pages become slow for all users, even those who don't use recommendations.

**Question:** What resilience pattern should the architect add to prevent recommendation service failures from affecting catalog performance?

A) Add more horizontal replicas of the recommendation service  
B) Implement a circuit breaker that opens when the AI provider failure rate exceeds a threshold, failing fast instead of waiting 30 seconds per request  
C) Reduce the retry count on the AI provider calls to 1  
D) Move the AI provider calls to a background queue  

**Answer:** B

**Why:** A circuit breaker detects aggregate failure rate and switches to "fail fast" mode — requests return immediately without waiting for the provider timeout, releasing threads immediately and preventing thread exhaustion from propagating to the catalog pages. A) more replicas don't help if all threads are blocked waiting on a slow dependency. C) fewer retries reduces total wait per request but doesn't prevent thread blocking on each attempt. D) a queue decouples calls but changes the UX from synchronous to deferred, which may not be acceptable for recommendations.

**Exam domain:** Design business continuity solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q3: Thundering herd — jitter role

**Scenario:** An AI gateway serves 500 concurrent users. The LLM provider returns 503s for 30 seconds during a brief overload. All 500 connections time out at approximately the same time. The retry policy re-sends all 500 requests simultaneously. The provider, still recovering, is immediately re-saturated.

**Question:** Which retry policy parameter prevents this re-saturation?

A) Setting `MaxRetryAttempts` to 1 — fewer retries means fewer simultaneous re-requests  
B) Using a fixed 5-second delay between retries for all callers  
C) Adding random jitter to the backoff delay — callers retry at different times, spreading load across the recovery window  
D) Setting `FailureRatio` to 90% so the circuit breaker absorbs most failures  

**Answer:** C

**Why:** Jitter randomises the retry delay per caller — instead of 500 requests hitting at T+5s simultaneously, they spread across T+3s to T+8s, allowing the recovering provider to process them progressively. A) fewer attempts reduce total retries but don't prevent simultaneous re-hits on the first retry. B) a fixed delay synchronises all callers at the same retry moment — this is worse than no delay in a thundering herd scenario. D) the circuit breaker addresses a different problem (sustained failure ratio, not synchronised retry timing).

**Exam domain:** Design business continuity solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q4: Attempt timeout configuration

**Scenario:** An AI gateway has a resilience pipeline with 3 retry attempts and a 30-second attempt timeout. The overall request timeout is 45 seconds. During load testing, callers frequently receive timeouts after exactly 45 seconds, even though the third retry had not finished.

**Question:** What configuration error caused this, and what is the fix?

A) The attempt timeout of 30 seconds is too short; increase it to 60 seconds  
B) The overall timeout (45s) is shorter than the maximum time for two full attempts (60s). Ensure `overall_timeout > (max_attempts × attempt_timeout)` to allow all retries to complete, or reduce the attempt timeout to fit within the budget  
C) The retry count of 3 is too high; reduce to 2  
D) Add a circuit breaker — timeouts indicate the circuit breaker is missing  

**Answer:** B

**Why:** With 3 retries × 30s attempt timeout = 90s maximum for all attempts, but the overall timeout fires at 45s — cutting off the second attempt mid-flight. The fix: ensure `overall_timeout > (max_attempts × attempt_timeout) + jitter_headroom`, or reduce the attempt timeout (e.g., 10s × 3 = 30s < 45s overall). A) increasing attempt timeout makes the mismatch worse. C) reducing retries may work numerically but doesn't fix the design flaw. D) a circuit breaker addresses different failure modes.

**Exam domain:** Design business continuity solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q5: Circuit breaker scope design

**Scenario:** An AI gateway routes requests to either Anthropic or Azure OpenAI based on model preference. A single circuit breaker monitors all upstream AI provider calls combined. When Anthropic experiences an outage, the combined failure rate opens the circuit, blocking Azure OpenAI calls too — which are healthy.

**Question:** What is the correct circuit breaker design?

A) Use a single wider circuit breaker with a higher failure ratio threshold (e.g., 80%)  
B) Disable the circuit breaker for Azure OpenAI and keep it only for Anthropic  
C) Create a separate circuit breaker per provider — one for Anthropic, one for Azure OpenAI — so each provider's failures affect only its own circuit  
D) Add more Azure OpenAI replicas so the combined failure rate stays below the threshold  

**Answer:** C

**Why:** Circuit breakers must be scoped to the specific dependency they protect. One breaker per provider ensures that Anthropic's outage only opens Anthropic's circuit — Azure OpenAI continues to serve requests normally, enabling automatic failover routing. A) raising the threshold delays the open but doesn't fix the cross-contamination. B) disabling the breaker on Azure OpenAI removes a resilience mechanism unnecessarily. D) more replicas don't address the root cause of the combined circuit breaker scope.

**Exam domain:** Design business continuity solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006
