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

//https://github.com/manuel284/ARM-AutomationAccountWithWebhook/blob/main/template.json
var tmpToken11 = uniqueString(subscription().id, resourceGroup().id, automationAccount.name, 'webhook1')
var tmpToken12 = uniqueString(resourceGroup().id,  automationAccount.name, 'webhook1')
var tmpToken21 = uniqueString(subscription().id, resourceGroup().id,  automationAccount.name, runbook.name)
var tmpToken22 = uniqueString(resourceGroup().id,  automationAccount.name, runbook.name)
var webhookTokenPart1 = substring(format('{0}{1}', tmpToken11,tmpToken12), 0, 20)
var webhookTokenPart2 = substring(format('{0}{1}', tmpToken21, tmpToken22), 0, 22)

resource webhook 'Microsoft.Automation/automationAccounts/webhooks@2018-06-30' = { //2023-11-01   2018-06-30
  parent: automationAccount
  name:  'webhook3'//format('{0}/{1}', automationAccount.name, 'webhook1')
  properties: {
    isEnabled: true
    expiryTime: '2035-01-01T00:00:00Z' 
    runbook: {
      name: runbook.name
    }
    parameters: {} 
    uri: format('{0}webhooks?token={1}%2b{2}%3d', 
      substring(replace(reference(resourceId('Microsoft.Automation/automationAccounts', automationAccount.name), '2023-11-01').automationHybridServiceUrl, '.jrds.', '.webhook.'), 0, indexOf(reference(resourceId('Microsoft.Automation/automationAccounts', automationAccount.name), '2023-11-01').automationHybridServiceUrl, '.azure-automation.net/') + 25), 
      webhookTokenPart1,
      webhookTokenPart2)
  }
}

output webhookUri string = webhook.properties.uri

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
        serviceUri: webhook.properties.uri
        useCommonAlertSchema: false
        automationAccountId: automationAccount.id
        runbookName: runbook.name
        webhookResourceId: webhook.id
        isGlobalRunbook: false
      }
    ]
  }
}

output actionGroupStopAzureFnsId string = actionGroup.id
