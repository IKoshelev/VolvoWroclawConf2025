targetScope='subscription'

@description('resourceGroupName will also be used as resource-prefix, lowercase letters, numbers and "-" allowed')
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
    location: 'eastus2' //hardcoded, since resources are available in less regions
    namePrefix: resourceGroupName
    alertEmail: alertEmail
    stopResourcesScriptPath: resources.outputs.stopResourcesScriptPath
    budgetStartDate: budgetStartDate
  }
}
