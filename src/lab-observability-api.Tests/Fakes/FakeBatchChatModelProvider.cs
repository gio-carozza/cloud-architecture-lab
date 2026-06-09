using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Services.AI;

namespace Lab.Observability.Api.Tests.Fakes;

public class FakeBatchChatModelProvider : IBatchChatModelProvider
{
    public string ProviderName => "fake";
    public Exception? ExceptionToThrow { get; set; }
    public void Reset() => ExceptionToThrow = null;

    private void ThrowIfSet()
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
    }

    public Task<BatchJob> SubmitBatchAsync(
        IReadOnlyList<ChatRequest> requests,
        CancellationToken ct)
    {
        ThrowIfSet();
        return Task.FromResult(new BatchJob
        {
            Id           = "fake-batch-id",
            Status       = BatchProcessingStatus.InProgress,
            RequestCount = requests.Count,
            CreatedAt    = DateTimeOffset.UtcNow,
            ExpiresAt    = DateTimeOffset.UtcNow.AddDays(1)
        });
    }

    public Task<BatchJobStatus> GetBatchStatusAsync(string id, CancellationToken ct)
    {
        ThrowIfSet();
        return Task.FromResult(new BatchJobStatus
        {
            Id             = id,
            Status         = BatchProcessingStatus.Ended,
            RequestCount   = 3,
            SucceededCount = 3
        });
    }

    public Task<IReadOnlyList<BatchResult>> GetBatchResultsAsync(string id, CancellationToken ct)
    {
        ThrowIfSet();
        IReadOnlyList<BatchResult> results = new List<BatchResult>
        {
            new() { CustomId = "request-0", IsSuccess = true,
                Response = new ChatResponse { Provider = "fake", Model = "fake-model", Response = "Result 0" } },
            new() { CustomId = "request-1", IsSuccess = true,
                Response = new ChatResponse { Provider = "fake", Model = "fake-model", Response = "Result 1" } },
        };
        return Task.FromResult(results);
    }
}
