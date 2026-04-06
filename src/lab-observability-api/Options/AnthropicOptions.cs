namespace Lab.Observability.Api.Options;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-opus-4-6";
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public int MaxTokens { get; set; } = 512;
}