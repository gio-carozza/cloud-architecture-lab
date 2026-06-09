namespace Lab.Observability.Api.Models.AI;

public class BatchJobStatus
{
    public string Id { get; set; } = string.Empty;
    public BatchProcessingStatus Status { get; set; }
    public int RequestCount { get; set; }
    public int SucceededCount { get; set; }
    public int ErroredCount { get; set; }
    public int CanceledCount { get; set; }
    public int ExpiredCount { get; set; }
}
