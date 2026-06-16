# Naming Conventions

## Owner suffix (`-gio`)

To avoid global naming collisions in shared Azure namespaces (DNS, App Insights,
Storage, Key Vault, etc.), all globally-unique resource types are suffixed
with `-gio`. This serves the same purpose a 3-letter org code would in
an enterprise (`acme-`, `con-`, etc.).

### When `-gio` is REQUIRED

Globally-unique Azure resources:

- App Service (`*.azurewebsites.net`)
- Storage Account (no hyphen — `staiassetsdevgio`)
- Key Vault (`*.vault.azure.net`)
- Application Insights
- SQL Server (`*.database.windows.net`)
- Cognitive Services / Azure OpenAI accounts
- Container Registry

### When `-gio` is RECOMMENDED (consistency)

Resources that are technically RG-scoped but benefit from cross-resource
queryability or where collision is still possible at scale:

- Log Analytics Workspace
- App Service Plan
- Action Group

### When `-gio` is OPTIONAL

Resources scoped to your own subscription where you own the namespace:

- Resource Group
- Budget
- Tag values

## Resource Groups

`rg-<project>-<env>-<region>`

Example:
`rg-ai-lab-dev-eastus`

Notes:

- No `-gio` needed (subscription-scoped, you own the namespace)
- Project: identifies the workload
- Environment: dev/test/prod
- Region: Azure region identifier

## App Service Plans

`asp-<project>-<env>-<region>-gio`

Example:
`asp-ai-lab-dev-eastus-gio`

## Web Apps (App Service)

`app-<project>-<role>-<env>-<region>-gio`

Example:
`app-ai-lab-api-dev-eastus-gio`

## Application Insights

`appi-<project>-<role>-<env>-<region>-gio`

Example:
`appi-ai-lab-api-dev-eastus-gio`

## Log Analytics Workspaces

`law-<project>-<env>-<region>-gio`

Example:
`law-ai-lab-dev-eastus-gio`

## Key Vaults

`kv-<project>-<env>-gio`

Example:
`kv-ai-secrets-dev-gio`

Note: Key Vault names cannot exceed 24 characters and cannot end with a hyphen.

## Storage Accounts

`st<project><env>gio`

Example:
`staiassetsdevgio`

Notes:

- No hyphens allowed in storage account names
- Lowercase only
- 3–24 characters
- Globally unique

## Action Groups

`ag-<purpose>-<env>-<region>-gio` for workload-scoped groups (e.g., AI gateway alerts)
`ag-<purpose>` for subscription-scoped groups (e.g., budget alerts)

Examples:

- `ag-ai-lab-dev-eastus-gio` — AI gateway alert receiver (workload-scoped, Day 6)
- `ag-cost-alerts` — budget threshold notifications (subscription-scoped)

## Budgets

`<purpose>-monthly-limit` (or similar)

Example:
`lab-monthly-limit`

Note: Budgets are subscription-scoped; suffix not required.

## Convention change log

| Date | Change | Reason |
|---|---|---|
| 2026-03 (Day 1) | Initial conventions established | Lab setup |
| 2026-04 (Day 6) | Added `-gio` ownership suffix to all globally-unique resources; clarified Log Analytics Workspace scoping | Naming collision encountered while provisioning `appi-ai-lab-dev-eastus`; standardized to prevent future collisions |
| 2026-04-30 (Day 6) | Relocated from `docs/notes/Day-001/` to `docs/standards/` | Folder structure should reflect document lifecycle, not authoring date; this file is a living standard, not a Day 1 artifact |
