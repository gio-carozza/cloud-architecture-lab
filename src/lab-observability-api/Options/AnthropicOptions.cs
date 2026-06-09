namespace Lab.Observability.Api.Options;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-6";
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";
    public int MaxTokens { get; set; } = 512;
    public bool EnablePromptCaching { get; init; } = true;
    public string SystemPrompt { get; init; } = "";
    public int MaxBatchSize { get; init; } = 100;
    public int MaxPromptLength { get; init; } = 32_000;
}