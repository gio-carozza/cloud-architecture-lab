namespace Lab.Observability.Api.Models.AI;

public class ChatResponse
{
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
}