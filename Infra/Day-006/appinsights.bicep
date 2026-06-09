@description('Environment name (dev, staging, prod)')
param environmentName string = 'dev'

@description('Azure region for all resources')
param location string = 'eastus'

@description('Owner suffix — appended to globally-unique resource names to avoid namespace collisions.')
param ownerSuffix string = 'gio'

@description('Email address that receives alert notifications.')
param alertEmail string

var workspaceName    = 'law-ai-lab-${environmentName}-${location}-${ownerSuffix}'
var appInsightsName  = 'appi-ai-lab-api-${environmentName}-${location}-${ownerSuffix}'
var actionGroupName  = 'ag-ai-lab-${environmentName}-${location}-${ownerSuffix}'
var alertRuleName    = 'alert-ai-gateway-5xx-rate-${environmentName}-${location}-${ownerSuffix}'

// ---------------------------------------------------------------------------
// Log Analytics Workspace — required backing store for workspace-based App Insights.
// Classic (non-workspace) App Insights cannot correlate traces with Log Analytics
// queries and lacks cross-resource KQL. Always workspace-based from day one.
// ---------------------------------------------------------------------------
resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ---------------------------------------------------------------------------
// Application Insights — workspace-based, kind 'web' (works for APIs too).
// WorkspaceResourceId wires it to the workspace above; ingestion goes to
// the workspace's Log Analytics tables (requests, dependencies, traces, etc.).
// ---------------------------------------------------------------------------
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// ---------------------------------------------------------------------------
// Action Group — defines WHERE alerts are sent.
// Location must be 'global' for action groups (not region-specific).
// useCommonAlertSchema: true standardises the email body across all alert types.
// ---------------------------------------------------------------------------
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: actionGroupName
  location: 'global'
  properties: {
    groupShortName: 'AILabAlert'
    enabled: true
    emailReceivers: [
      {
        name: 'PrimaryEmail'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

// ---------------------------------------------------------------------------
// Scheduled Query Alert — fires when 5xx rate exceeds 5% in any 5-min window.
//
// KQL design: query always returns one row with failureRate (0–100).
// The alert evaluates avg(failureRate) > 5, which is equivalent to the single
// value in that one row. If the window has zero requests, failureRate = 0
// and no alert fires — avoids false positives during quiet periods.
//
// Severity 2 = Warning (0 = Critical, 1 = Error, 2 = Warning, 3 = Informational).
// Severity 2 is appropriate here: elevated error rate is worth investigating but
// does not automatically mean an outage.
// ---------------------------------------------------------------------------
resource alertRule 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: alertRuleName
  location: location
  properties: {
    description: 'Fires when 5xx responses exceed 5% of total requests in any 5-minute window.'
    severity: 2
    enabled: true
    scopes: [appInsights.id]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          query: '''
            requests
            | summarize
                total    = count(),
                failures = countif(toint(resultCode) >= 500)
            | extend failureRate = iif(total > 0,
                (todouble(failures) / todouble(total)) * 100.0,
                0.0)
          '''
          metricMeasureColumn: 'failureRate'
          timeAggregation: 'Average'
          operator: 'GreaterThan'
          threshold: 5
        }
      ]
    }
    actions: {
      actionGroups: [actionGroup.id]
    }
  }
}

// ---------------------------------------------------------------------------
// Outputs — consumed by App Service Bicep (Day 7+) and CI/CD pipelines.
// ConnectionString is the preferred value; InstrumentationKey is legacy but
// still required by some older SDKs.
// ---------------------------------------------------------------------------
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output workspaceId string = workspace.id
output appInsightsId string = appInsights.id
