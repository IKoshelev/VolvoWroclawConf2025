targetScope='subscription'

@description('The prefix to use for the storage account name will also be used as resource-prefix')
param resourceGroupName string
param resourceGroupLocation string

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
}

module m 'Resources.bicep' = {
  name: 'resourceModule'
  scope: rg
  params: {
    location: rg.location
    namePrefix: resourceGroupName
  }
}
