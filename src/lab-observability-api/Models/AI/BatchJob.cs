namespace Lab.Observability.Api.Models.AI;

public class BatchJob
{
    public string Id { get; set; } = string.Empty;
    public BatchProcessingStatus Status { get; set; }
    public int RequestCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
