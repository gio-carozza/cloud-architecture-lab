using System.Runtime.CompilerServices;
using Lab.Observability.Api.Models.AI;

namespace Lab.Observability.Api.Services.AI;

public interface IChatModelProvider
{
    Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default);

    async IAsyncEnumerable<ChatChunk> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var response = await SendAsync(request, ct);
        yield return new ChatChunk(response.Response, "end_turn", null);
    }
}