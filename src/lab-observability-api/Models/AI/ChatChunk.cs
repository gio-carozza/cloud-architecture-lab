namespace Lab.Observability.Api.Models.AI;

public record ChatChunk(
    string TextDelta,
    string? StopReason,
    ChatChunkUsage? Usage);

public record ChatChunkUsage(
    int InputTokens,
    int OutputTokens,
    int CacheReadTokens,
    int CacheCreationTokens);
