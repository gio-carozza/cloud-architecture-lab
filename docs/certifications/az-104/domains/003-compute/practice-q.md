# Practice Questions — Deploy and manage Azure compute resources

## Day 007 additions (2026-06-03)
*5 questions — App Service configuration, operational toggles, ARM settings management*

---

## Q1: Double-underscore setting binding

**Scenario:** You are configuring an Azure App Service running a .NET 8 application. The app reads configuration via `IConfiguration` using the key `Anthropic:ApiKey`. You need to set this value in the App Service application settings without modifying the source code.

**Question:** What should you name the App Service application setting?

A) `Anthropic:ApiKey`
B) `Anthropic__ApiKey`
C) `Anthropic-ApiKey`
D) `AnthropicApiKey`

**Answer:** B

**Why:** App Service injects application settings as environment variables. Environment variable names cannot contain colons on Linux (and many Windows shells). Azure App Service translates `__` (double underscore) to `:` when surfacing values through `IConfiguration`. Options A uses a colon (invalid in env var names), C uses a hyphen (not the IConfiguration separator), and D removes the hierarchy entirely.

**Exam domain:** Deploy and manage Azure compute resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q2: Changing a setting without redeployment

**Scenario:** Your company's AI gateway is deployed to Azure App Service. A new prompt caching feature was deployed last week with `Anthropic__EnablePromptCaching=true`. The feature is causing unexpected latency and you need to disable it immediately without redeploying the application.

**Question:** What is the fastest way to disable the feature?

A) Modify the `appsettings.json` file in the source repository and trigger a CI/CD pipeline
B) Change the `Anthropic__EnablePromptCaching` App Service application setting to `false` and save
C) Swap to the staging deployment slot
D) Scale the App Service plan to a higher tier

**Answer:** B

**Why:** App Service application settings changes take effect after a rolling restart of the app instances — no build, artifact, or pipeline required. This is the purpose of the operational toggle pattern. Option A requires a full CI/CD cycle. Option C swaps slots but doesn't change the setting value; the same value would be live in staging. Option D changes compute capacity, not configuration.

**Exam domain:** Deploy and manage Azure compute resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q3: App Service settings override behavior

**Scenario:** A .NET application has `"Anthropic": { "Model": "claude-3-opus" }` in its `appsettings.json`. The Azure App Service has an application setting named `Anthropic__Model` with value `claude-sonnet-4-6`. What model name does the running application read?

**Question:** Which value does `IConfiguration["Anthropic:Model"]` return at runtime?

A) `claude-3-opus` — appsettings.json takes precedence
B) `claude-sonnet-4-6` — App Service settings override appsettings.json
C) An exception, because both sources define the same key
D) `null` — duplicate keys are silently discarded

**Answer:** B

**Why:** In the .NET configuration system, environment variables (which App Service settings become) are registered after file-based providers and therefore override them. This is the intended behavior — App Service settings are the production-environment layer that overrides development defaults in committed files. Options A, C, and D misrepresent how the configuration priority chain works.

**Exam domain:** Deploy and manage Azure compute resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q4: Storing secrets in App Service

**Scenario:** Your team's App Service requires an API key to call a third-party AI provider. A junior developer has added the key to `appsettings.Production.json` and committed it to the repository.

**Question:** What is the correct remediation?

A) Move the key to `appsettings.Development.json` so it is only used locally
B) Encrypt the key value in `appsettings.Production.json` using Base64 encoding
C) Remove the key from the file, store it as an App Service application setting, and rotate the compromised key
D) Add the file to `.gitignore` going forward but leave the current commit as-is

**Answer:** C

**Why:** The secret is already compromised the moment it was committed — rotation is mandatory. The correct storage for secrets in App Service is application settings (or Key Vault references), not files committed to source control. Option A merely moves the problem. Option B is security theater — Base64 is not encryption and the key is still exposed in git history. Option D prevents future leakage but does not remediate the current exposure or fix the architectural problem.

**Exam domain:** Deploy and manage Azure compute resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q5: WEBSITE_RUN_FROM_PACKAGE behavior

**Scenario:** You are deploying a .NET application to Azure App Service using a zip file via the Kudu publish API. After deploy, the application returns 503 errors. Checking the App Service logs reveals the startup fails with "cannot write to the application directory."

**Question:** Which configuration is most likely missing?

A) `ASPNETCORE_ENVIRONMENT=Production`
B) `WEBSITE_RUN_FROM_PACKAGE=1`
C) `SCM_DO_BUILD_DURING_DEPLOYMENT=false`
D) `WEBSITE_NODE_DEFAULT_VERSION=18`

**Answer:** B

**Why:** `WEBSITE_RUN_FROM_PACKAGE=1` tells App Service to mount the zip as a read-only virtual filesystem rather than extracting it to the `wwwroot` directory. Without this setting, App Service may attempt to write to the app directory during startup, which can fail on certain App Service plans or trigger security restrictions. This is the required setting for Kudu zip deployments. Options A, C, and D address different concerns (environment, SCM build pipeline, Node version) and would not cause this error.

**Exam domain:** Deploy and manage Azure compute resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---
