using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Services.Claude;

namespace Lab.Observability.Api.Services.AI;

public sealed class ClaudeBatchChatModelProvider : IBatchChatModelProvider
{
    private readonly ClaudeBatchApiClient _client;

    public ClaudeBatchChatModelProvider(ClaudeBatchApiClient client)
    {
        _client = client;
    }

    public string ProviderName => "anthropic-batch";

    public Task<BatchJob> SubmitBatchAsync(
        IReadOnlyList<ChatRequest> requests,
        CancellationToken cancellationToken = default)
        => _client.SubmitAsync(requests, cancellationToken);

    public Task<BatchJobStatus> GetBatchStatusAsync(
        string batchJobId,
        CancellationToken cancellationToken = default)
        => _client.GetStatusAsync(batchJobId, cancellationToken);

    public Task<IReadOnlyList<BatchResult>> GetBatchResultsAsync(
        string batchJobId,
        CancellationToken cancellationToken = default)
        => _client.GetResultsAsync(batchJobId, cancellationToken);
}
