namespace Lab.Observability.Api.Models.AI;

public class BatchResult
{
    public string CustomId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public ChatResponse? Response { get; set; }
    public string? ErrorMessage { get; set; }
}
