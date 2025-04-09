param namePrefix string = resourceGroup().name
param location string = resourceGroup().location
param alertEmail string
param stopResourcesScriptPath string
param budgetStartDate string = '2025-04-01 00:00:00Z'

resource automationAccount 'Microsoft.Automation/automationAccounts@2023-11-01' = {
  name:  '${namePrefix}-automationAccount'
  location: location
  properties: {
    sku: {
      name: 'Basic'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource roleAssignment_contributorForRG 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(automationAccount.id, 'b24988ac-6180-42a0-ab88-20f7382dd24c') // Contributor role ID
  scope: resourceGroup()
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
    principalId: automationAccount.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource runbook 'Microsoft.Automation/automationAccounts/runbooks@2023-11-01' = {
  name: '${namePrefix}-automationRunbook'
  parent: automationAccount
  location: location
  properties: {
    runbookType: 'PowerShell72'
    description: 'Stops Azure Functions'
    logProgress: true
    logVerbose: true
    publishContentLink: {
      uri: stopResourcesScriptPath
    }
  }
}

resource webhook 'Microsoft.Automation/automationAccounts/webhooks@2018-06-30' = { //only 2018 version seems to work
  parent: automationAccount
  name:  'stopResourcesRunbookWebhook'
  properties: {
    isEnabled: true
    expiryTime: '2035-01-01T00:00:00Z' 
    runbook: {
      name: runbook.name
    }
    parameters: {} 
  }
}

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${namePrefix}-budgetGuardActionGroup'
  location: 'global'
  properties: {
    enabled: true
    groupShortName: 'budgetGuard'
    emailReceivers: [
      {
        name: 'email_-EmailAction-'
        emailAddress: alertEmail
        useCommonAlertSchema: false
      }
    ]
    automationRunbookReceivers: [
      {
        name: 'stop azure fns'
        serviceUri: reference(webhook.name).uri // AFAIK, this is the only way to get the hook uri, and it's not available after creation for security reasons
        useCommonAlertSchema: false
        automationAccountId: automationAccount.id
        runbookName: runbook.name
        webhookResourceId: webhook.id
        isGlobalRunbook: false
      }
    ]
  }
}

resource budget 'Microsoft.Consumption/budgets@2023-11-01' = {
  name: '${resourceGroup().name}-budget'
  properties: {
    timeGrain: 'Monthly'
    amount: 10
    category: 'Cost'
    timePeriod: {
      startDate:  budgetStartDate
      //endDate: endDate
    }
    notifications: {
      NotificationForExceededBudget1: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 95
        thresholdType: 'Actual'
        contactEmails: [alertEmail]
        contactGroups: [
          actionGroup.id
        ]
      }
    }
    filter: {
      dimensions: {
        name: 'ResourceGroupName'
        operator: 'In'
        values: [resourceGroup().name]
      }
    }
  }
}

