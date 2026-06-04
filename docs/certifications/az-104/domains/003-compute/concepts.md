# Concepts — Deploy and manage Azure compute resources

## Day 007 additions (2026-06-03)
*Topics: Azure App Service application settings, operational toggle pattern, ARM API for settings management*

---

## Azure App Service Application Settings

### If you're 10 years old
Imagine your app is like a vending machine. The machine needs to know the price of each item, but you don't want to open it up every time you change a price. App Service application settings are like a label on the outside — you change what it says without opening the machine, and the machine reads the new value automatically when it restarts.

### If you're a CEO
App Service settings are the control panel for your application without touching code. Need to rotate an API key? Change a setting. Switch the app to point at a different database? Change a setting. Each change takes seconds and triggers a rolling restart with zero downtime. The alternative — baking configuration into the application and redeploying — takes hours and introduces deployment risk. Settings are a competitive advantage in how fast you can respond to operational changes.

### If you're an Engineer
Settings are injected as OS environment variables at startup and read by `IConfiguration` automatically. Use double underscore `__` as the hierarchy separator: `Anthropic__ApiKey` becomes `Anthropic:ApiKey` in IConfiguration, enabling binding to `IOptions<AnthropicOptions>`. Read via `builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection("Anthropic"))`. Never read `IConfiguration` directly in business logic — always bind to a typed options class. Set in Azure Portal, `az webapp config appsettings set`, or via ARM PATCH. Common mistake: using `:` in setting names instead of `__` — Azure Portal accepts both, but the `az` CLI and ARM API require `__` for nested config.

### If you're an Architect
App Service application settings are key-value pairs injected into the application process as OS environment variables at startup. They override any matching keys in `appsettings.json`, making them the canonical configuration plane for secrets, connection strings, and operational parameters in production. The .NET `IConfiguration` system reads them automatically; the `__` (double underscore) separator in a setting name translates to `:` in IConfiguration, enabling structured configuration binding (e.g., `Anthropic__ApiKey` → `Anthropic:ApiKey`).

**Why it matters in enterprise:** Settings are the deployment-decoupled control plane. Changing a setting triggers a rolling restart with no new build, no new artifact, no pipeline run. This makes them the correct mechanism for secrets rotation, provider endpoint swaps, and operational toggles. Putting operational values in `appsettings.json` violates secrets hygiene and removes the ability to change values without a full CI/CD cycle.

**Common beginner mistake:** Using `appsettings.json` for environment-specific values instead of App Service settings, then wondering why staging and production share the same configuration. Environment-specific values belong in the platform's configuration plane, never in files committed to a repository.

---

## Operational Toggle Pattern via App Service Settings

### If you're 10 years old
A light switch lets you turn lights on or off without rewiring your house. An operational toggle does the same thing for a feature in your app — flip a setting on the dashboard, the app restarts in seconds, the feature is on or off. No new code. No deployment.

### If you're a CEO
When something goes wrong at 3 AM — an AI feature is generating bad responses, or a cost spike appears — your on-call engineer needs a lever that doesn't require waking up a developer to deploy code. Operational toggles are that lever. Flip one setting and the feature is disabled in seconds. This is the difference between a 2-minute incident response and a 45-minute emergency deploy. For AI products where a bad model output is a brand risk, this lever is not optional.

### If you're an Engineer
Add a boolean field to your options class (`AnthropicOptions.EnablePromptCaching`), bind it via `IOptions<AnthropicOptions>`, and branch on it in the executing code path. The setting name in App Service: `Anthropic__EnablePromptCaching=false`. This disables the feature without a code deployment. Key requirement: the toggle must gate the feature at the right granularity — not so coarse it disables the entire service, not so fine it requires 10 toggles to disable one feature. Test that setting `false` actually disables the feature, and that the app logs which state it started in (observable on `/health/info`).

### If you're an Architect
An operational toggle is an application setting that gates a code path at runtime. Pattern: add a boolean to an options class (`AnthropicOptions.EnablePromptCaching`), bind it via `IOptions<T>`, branch on the value in the executing path. Setting `Anthropic__EnablePromptCaching=false` on the App Service immediately disables caching without redeployment.

This is a primitive feature flag — appropriate for infrastructure-level toggles (provider switches, resilience modes, cost controls) where the flag is owned by the platform team. For AI gateways, the ability to disable prompt caching, switch model tiers, or revert to a fallback provider via a setting change (no code deploy) is a production resilience primitive.

**Why it matters in enterprise:** When a cost anomaly appears at 3 AM, the on-call engineer needs a lever that does not require a deployment. App Service settings are that lever.

**Common beginner mistake:** Hard-coding operational gates as compiler constants. These require a build and deploy to change — useless as emergency levers. Operational toggles must be runtime-configurable.

---

## ARM REST API for App Service Settings Management

### If you're 10 years old
Normally you manage your app using the Azure portal, like clicking buttons on a control panel. The ARM API lets programs do the same thing. But sometimes the building's security system (the network) blocks certain types of requests — and you have to find out which ones are allowed and work around the rest.

### If you're a CEO
Your CI/CD pipeline and automation scripts manage Azure through the ARM API — the same API the portal uses. If your network's security inspection blocks certain API calls (a common enterprise constraint), your automation silently fails. This isn't an Azure bug or a code bug — it's a network policy. Knowing this distinction means your team diagnoses the root cause in minutes instead of hours, and routes around it rather than guessing at fixes.

### If you're an Engineer
Two paths for settings writes: (1) `PUT .../sites/{name}/config/appsettings` — full replace, must include ALL existing settings or they are deleted; (2) `PATCH .../sites/{name}` with body `{properties:{siteConfig:{appSettings:[...]}}}` — also replaces `appSettings` in full, but only touches the site-level properties you include. Auth: acquire a bearer token via `az account get-access-token --query accessToken -o tsv` and pass as `Authorization: Bearer <token>` header. If `az webapp config appsettings set` fails silently on your network, it's likely a TLS inspection block on `PUT management.azure.com`. Switch to `Invoke-RestMethod -Method PATCH` — `PATCH` transits network inspection layers that block `PUT` on many enterprise proxies.

### If you're an Architect
App Service settings are managed via the ARM REST API. Two paths:

**Full-replace PUT:**
```
PUT .../sites/{name}/config/appsettings
```
Replaces the *entire* settings collection. Any setting omitted from the body is deleted. Must include all existing settings.

**Site-level PATCH:**
```
PATCH .../sites/{name}
body: { properties: { siteConfig: { appSettings: [{name,value},...] } } }
```
Also replaces `siteConfig.appSettings` in full — the "partial" refers to which site properties are updated, not individual settings.

**Network constraint (Day 7 observation):** TLS inspection proxies commonly block `PUT` to `management.azure.com` while allowing `POST` reads and `PATCH`. The `az webapp config appsettings set` CLI uses PUT internally and fails silently on such networks. `Invoke-RestMethod PATCH` to the site resource is the tested alternative.

**Why it matters in enterprise:** Any automation that writes App Service settings must account for network-layer restrictions. Standard pattern: read via `/config/appsettings/list` (POST action, works broadly), write via PATCH or the Azure portal. Test ARM write paths from the same network as your CI/CD pipeline.

**Common beginner mistake:** Assuming `az` CLI failures are Azure-side bugs. TLS inspection drops connections silently. Always isolate network vs. API issues before debugging the tool.

---
