targetScope='subscription'

@description('The prefix to use for the storage account name will also be used as resource-prefix')
param resourceGroupName string
param resourceGroupLocation string
param alertEmail string
param budgetStartDate string = '2025-04-01 00:00:00Z'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
}

module resources 'Resources.bicep' = {
  name: 'resourceModule'
  scope: rg
  params: {
    location: rg.location
    namePrefix: resourceGroupName
  }
}

module budgetGuard 'BudgetGuard.bicep' = {
  name: 'budgetGuardModule'
  scope: rg
  params: {
    location: 'eastus2'
    namePrefix: resourceGroupName
    alertEmail: alertEmail
    stopResourcesScriptPath: resources.outputs.stopResourcesScriptPath
  }
}

resource budget 'Microsoft.Consumption/budgets@2021-10-01' = {
  name: '${resourceGroupName}-budget'
  properties: {
    timeGrain: 'Monthly'
    amount: 20
    category: 'Cost'
    timePeriod: {
      startDate:  budgetStartDate
      //endDate: endDate
    }
    notifications: {
      NotificationForExceededBudget1: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 1000
        contactEmails: [alertEmail]
      }
    }
    filter: {
      dimensions: {
        name: 'ResourceGroupName'
        operator: 'In'
        values: [resourceGroupName]
      }
    }
  }
}
