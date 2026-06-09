using System.Runtime.CompilerServices;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Services.AI;

namespace Lab.Observability.Api.Tests.Fakes;

public class FakeChatModelProvider : IChatModelProvider
{
    public Exception? ExceptionToThrow { get; set; }

    public Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken ct = default)
    {
        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        return Task.FromResult(new ChatResponse
        {
            Provider = "fake",
            Model = "fake-model",
            Response = "Fake response."
        });
    }

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        yield return new ChatChunk("Fake ", null, null);
        await Task.Yield();
        yield return new ChatChunk("response.", "end_turn",
            new ChatChunkUsage(10, 5, 0, 0));
    }

    public void Reset() => ExceptionToThrow = null;
}
