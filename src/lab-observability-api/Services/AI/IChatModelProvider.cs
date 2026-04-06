using Lab.Observability.Api.Models.AI;

namespace Lab.Observability.Api.Services.AI;

public interface IChatModelProvider
{
    Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default);
}