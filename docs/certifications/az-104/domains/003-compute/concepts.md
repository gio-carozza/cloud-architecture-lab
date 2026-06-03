# Concepts — Deploy and manage Azure compute resources

## Day 007 additions (2026-06-03)
*Topics: Azure App Service application settings, operational toggle pattern, ARM API for settings management*

---

## Azure App Service Application Settings

### If you're 10 years old
Imagine your app is like a vending machine. The machine needs to know the price of each item, but you don't want to open it up every time you change a price. App Service application settings are like a label on the outside — you change what it says without opening the machine, and the machine reads the new value automatically when it restarts.

### If you're an architect
App Service application settings are key-value pairs injected into the application process as OS environment variables at startup. They override any matching keys in `appsettings.json`, making them the canonical configuration plane for secrets, connection strings, and operational parameters in production. The .NET `IConfiguration` system reads them automatically; the `__` (double underscore) separator in a setting name translates to `:` in IConfiguration, enabling structured configuration binding (e.g., `Anthropic__ApiKey` → `Anthropic:ApiKey`).

**Why it matters in enterprise:** Settings are the deployment-decoupled control plane. Changing a setting triggers a rolling restart with no new build, no new artifact, no pipeline run. This makes them the correct mechanism for secrets rotation, provider endpoint swaps, and operational toggles. Putting operational values in `appsettings.json` violates secrets hygiene and removes the ability to change values without a full CI/CD cycle.

**Common beginner mistake:** Using `appsettings.json` for environment-specific values instead of App Service settings, then wondering why staging and production share the same configuration. Environment-specific values belong in the platform's configuration plane, never in files committed to a repository.

---

## Operational Toggle Pattern via App Service Settings

### If you're 10 years old
A light switch lets you turn lights on or off without rewiring your house. An operational toggle does the same thing for a feature in your app — flip a setting on the dashboard, the app restarts in seconds, the feature is on or off. No new code. No deployment.

### If you're an architect
An operational toggle is an application setting that gates a code path at runtime. Pattern: add a boolean to an options class (`AnthropicOptions.EnablePromptCaching`), bind it via `IOptions<T>`, branch on the value in the executing path. Setting `Anthropic__EnablePromptCaching=false` on the App Service immediately disables caching without redeployment.

This is a primitive feature flag — appropriate for infrastructure-level toggles (provider switches, resilience modes, cost controls) where the flag is owned by the platform team. For AI gateways, the ability to disable prompt caching, switch model tiers, or revert to a fallback provider via a setting change (no code deploy) is a production resilience primitive.

**Why it matters in enterprise:** When a cost anomaly appears at 3 AM, the on-call engineer needs a lever that does not require a deployment. App Service settings are that lever.

**Common beginner mistake:** Hard-coding operational gates as compiler constants. These require a build and deploy to change — useless as emergency levers. Operational toggles must be runtime-configurable.

---

## ARM REST API for App Service Settings Management

### If you're 10 years old
Normally you manage your app using the Azure portal, like clicking buttons on a control panel. The ARM API lets programs do the same thing. But sometimes the building's security system (the network) blocks certain types of requests — and you have to find out which ones are allowed and work around the rest.

### If you're an architect
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
