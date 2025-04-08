param namePrefix string = resourceGroup().name
param location string = resourceGroup().location

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: '${namePrefix}-db'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    //enableFreeTier: true // Enables free-tier 1000 RU/s
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-11-15' = {
  parent: cosmosDbAccount
  name: 'PWA_DB'
  properties: {
    resource: {
      id: 'PWA_DB'
    }
    options: {
      throughput: 1000 
    }
  }
}

resource _1 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-12-01-preview' = {
  parent: cosmosDbDatabase
  name: 'DelayedNotification'
  properties: {
    resource: {
      id: 'DelayedNotification'
      partitionKey: {
        paths: [
          '/DelayedNotificationId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

resource _2 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-12-01-preview' = {
  parent: cosmosDbDatabase
  name: 'User'
  properties: {
    resource: {
      id: 'User'
      partitionKey: {
        paths: [
          '/UserId'
        ]
        kind: 'Hash'
        version: 2
      }
    }
  }
}

var managementScriptsContainerName = 'managementscripts'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${namePrefix}storage'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  resource blobService 'blobServices' = {
    name: 'default'

    resource container 'containers' = {
      name: managementScriptsContainerName
    }
  }
}

var filename = 'stop-azure-fn-resources.ps1'
var stopResourcesScript = replace('''
Connect-AzAccount -Identity

Get-AzFunctionApp -ResourceGroupName "resource-group-name" | ForEach-Object { 
    Stop-AzFunctionApp -Force -Name $_.Name -ResourceGroupName "resource-group-name" 
}
''', 'resource-group-name', resourceGroup().name)

resource deploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${namePrefix}-deploy-${managementScriptsContainerName}-blob'
  location: location
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.55.0'
    timeout: 'PT5M'
    retentionInterval: 'PT1H'
    environmentVariables: [
      {
        name: 'AZURE_STORAGE_ACCOUNT'
        value: storageAccount.name
      }
      {
        name: 'AZURE_STORAGE_KEY'
        secureValue: storageAccount.listKeys().keys[0].value
      }
      {
        name: 'CONTENT'
        value: stopResourcesScript
      }
    ]
    scriptContent: 'echo "$CONTENT" > ${filename} && az storage blob upload -f ${filename} -c ${managementScriptsContainerName} -n ${filename}'
  }
}

output stopResourcesScriptPath string = '${storageAccount.properties.primaryEndpoints.blob}${managementScriptsContainerName}/${filename}'

resource serverAzureFnPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: '${namePrefix}-api-appServerPlan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
}

resource serverAzureFn 'Microsoft.Web/sites@2024-04-01' = {
  name: '${namePrefix}-api'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: serverAzureFnPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccount.properties.primaryEndpoints.blob
        }
      ]
    }
  }
}

resource staticWebApp 'Microsoft.Web/staticSites@2024-04-01' = {
  name: '${namePrefix}-staticWebApp'
  location: location //may need separate location, as not everywhere available
  properties: {
    deploymentAuthPolicy: 'DeploymentToken'
    enterpriseGradeCdnStatus: 'Disabled'
  }
  sku: {
    name: 'Free'
    tier: 'Free'
  }
}
