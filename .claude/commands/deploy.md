# /deploy

Deploy `lab-observability-api` to Azure App Service using the proven Kudu zip path.

## Usage
`/deploy`

## What this does

Read `.claude/skills/azure-deploy/SKILL.md` and execute the steps:

1. Verify pre-flight (subscription, RG, app exists)
2. Run `dotnet publish -c Release -o ./publish` from the API project
3. Zip with `Compress-Archive -Path .\publish\*` (files at root)
4. Ensure `WEBSITE_RUN_FROM_PACKAGE=1` is set
5. Acquire ARM token
6. POST to Kudu publish API
7. Verify `/health`, `/swagger`, and `POST /api/ai/chat` post-deploy

## DO NOT
- Use `az webapp deploy` (known to fail on this network)
- Zip the `publish` folder itself (must be `publish\*`)
- Deploy without verifying app settings include `Anthropic__*` keys

## Output
Report each step's status. If any step fails, halt and surface the exact error.