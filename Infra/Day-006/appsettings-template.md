# Day 006 App Settings Template

## Local development

Use user secrets for real local secrets.

Recommended user secrets:

## powershell

dotnet user-secrets set "Anthropic:ApiKey" "<your-real-anthropic-key>"
dotnet user-secrets set "Anthropic:Model" "claude-opus-4-7"
dotnet user-secrets set "Anthropic:BaseUrl" "https://api.anthropic.com/v1"
dotnet user-secrets set "Anthropic:MaxTokens" "512"
dotnet user-secrets set "APPLICATIONINSIGHTS_CONNECTION_STRING" "<your-real-app-insights-connection-string>"