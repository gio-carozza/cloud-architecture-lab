using Lab.Observability.Api.Models.AI;

namespace Lab.Observability.Api.Services.AI;

public interface IBatchChatModelProvider
{
    Task<BatchJob> SubmitBatchAsync(IReadOnlyList<ChatRequest> requests, CancellationToken cancellationToken = default);
    Task<BatchJobStatus> GetBatchStatusAsync(string batchJobId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResult>> GetBatchResultsAsync(string batchJobId, CancellationToken cancellationToken = default);
    string ProviderName { get; }
}
