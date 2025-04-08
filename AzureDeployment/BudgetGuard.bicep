param namePrefix string = resourceGroup().name
param location string = resourceGroup().location
param alertEmail string
param stopResourcesScriptPath string

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

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${namePrefix}-budgetGuardActionGroup'
  location: 'global' //location // available regions limited
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
        name: 'stop resources'
        serviceUri: 'https://bf1091a2-21de-4f2e-bff1-5fa6a8b22064.webhook.plc.azure-automation.net/webhooks?token=zNxQ9UPFVnt06c5A4yLNox%2bMjBrVo16FKj3o7HyBmTQ%3d'
        useCommonAlertSchema: false
        automationAccountId: automationAccount.id
        runbookName: 'stop-volvo-wroclaw-conf-2025-resources'
        webhookResourceId: '${automationAccount.id}/webhooks/Alert1743555810678'
        isGlobalRunbook: false
      }
    ]
  }
}
