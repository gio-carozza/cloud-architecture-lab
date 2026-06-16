# RAG Architecture Patterns

**Phase:** 2 stub — reference now, implement starting Phase 2
**Applies to:** any feature that grounds LLM responses in external data sources

---

## Why RAG matters for this gateway

The gateway currently answers questions from the model's training data only. RAG (Retrieval-Augmented Generation) connects the model to live, organization-specific, or private data. Without RAG, the gateway cannot power use cases that require current or proprietary information — which is the majority of enterprise AI use cases.

Adding RAG is the first Phase 2 feature. This document is a Phase 1 design stub so that the gateway's architecture does not have to be redesigned when Phase 2 begins.

---

## RAG architecture — first generation (Phase 2 start)

```text
User query
    ↓
[Query encoder] → embedding vector
    ↓
[Vector store: Azure AI Search] ← document chunks + embeddings
    ↓
[Top-K retrieved chunks]
    ↓
[Context assembler] → system prompt + retrieved chunks + query
    ↓
[IChatModelProvider.SendAsync]
    ↓
[Response with citations]
```

The gateway's `IChatModelProvider` seam is already in place. RAG extends the **context assembly** step — it does not change the provider interface.

---

## RAG architecture — second generation (Phase 2 mid)

**Agentic RAG** — the model decides when and what to retrieve, rather than the pipeline always retrieving unconditionally.

```text
User query
    ↓
[Router: does this query need retrieval?]
    ↓ yes                   ↓ no
[Query rewriter]       [Direct to model]
    ↓
[Multi-hop retriever] ← vector store
    ↓
[Reranker: Cohere Rerank or FlashRank]
    ↓
[Grounded response + citations]
```

**GraphRAG** (Microsoft, 2024) — graph-structured knowledge base for multi-hop questions. When the question requires connecting information across multiple documents, flat vector retrieval fails. GraphRAG stores entity relationships in a graph and traverses it during retrieval.

---

## Azure AI Search integration plan

Service: `srch-ai-lab-dev-eastus-gio` (to be created in Phase 2, Day 21)

| Capability | Azure AI Search tier | Notes |
|---|---|---|
| Vector search | Basic or Standard | Built-in; no separate vector DB needed |
| Hybrid search (BM25 + vector) | Standard | Keyword + semantic in one query |
| Semantic reranking | Standard S1+ | Azure's L2 reranker |
| Integrated vectorization | Standard | Auto-embed during document ingest |

Naming follows repo convention: `srch-ai-lab-dev-eastus-gio` (globally unique, ends with `-gio`).

---

## Chunking strategy

Chunking determines retrieval quality more than any other single decision.

| Strategy | When to use | Chunk size |
|---|---|---|
| Fixed-size | Simple prose documents | 512–1024 tokens |
| Semantic | Documents with natural section breaks | Variable; split at headings/paragraphs |
| Sentence window | Q&A over precise factual content | ±2 sentences around the retrieved sentence |
| Hierarchical | Long documents with structure (manuals, specs) | Summary + detail; retrieve at summary, expand on hit |

For Phase 2 start: semantic chunking at heading/paragraph boundaries with 512-token max and 50-token overlap.

---

## Retrieval quality checks

Before any RAG feature ships, verify:

- **Recall@K** — does the correct chunk appear in the top-K results for 80%+ of test queries?
- **Groundedness** — does the model response make claims supported by the retrieved chunks?
- **Citation accuracy** — do cited sources actually contain the stated information?

Use the `eval-framework.md` golden test set structure extended with `(query, expected_source_chunk, grading_rubric)` triples.

---

## Context assembly rules

When assembling the prompt with retrieved context:

1. Retrieved chunks go in the system prompt as context (not in the user message) — this allows prompt caching on the system prompt if the same document set is frequently queried
2. Include source metadata (document title, section, date) with each chunk — the model needs this to generate citations
3. Instruct the model explicitly to cite sources and to say "I don't know" when the retrieved context doesn't support an answer
4. Cap retrieved context at 60% of the context window budget — leave room for conversation history and output

---

## Anti-patterns to avoid

| Anti-pattern | Problem | Fix |
|---|---|---|
| Retrieving too many chunks (K > 10) | Degrades response quality; model loses focus | Start with K=3–5; tune with eval |
| No reranking step | BM25+vector returns noisy results | Add `Cohere Rerank` or Azure semantic reranker |
| Cached embeddings out of sync | Stale chunks cause hallucinations about live data | Implement document change detection + re-embedding pipeline |
| Same retrieval path for all query types | Complex multi-hop queries fail with flat retrieval | Classify query type; route to appropriate retrieval strategy |
| Storing raw PII in the vector index | Compliance violation | Apply PII detection before indexing (see `responsible-ai.md`) |

---

## ADR requirement

Creating the RAG layer is an ADR-worthy decision. The ADR must address:

- Choice of vector store (Azure AI Search vs. `pgvector` vs. `Qdrant` vs. `Weaviate`)
- Chunking strategy rationale
- Reranking approach
- Citation format in API responses
- Impact on `ChatRequest`/`ChatResponse` contracts (citations are a new field)

Draft this ADR as Day 21's first artifact.
