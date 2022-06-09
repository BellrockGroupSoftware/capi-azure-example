@description('Environment type being deployed')
@allowed([
  'Dev'
  'Test'
  'Live'
])
param environmentName string = 'Dev'

param resourceGroupLocation string = resourceGroup().location

/*
  APP STORAGE
*/

module appStorage 'appStorage.bicep' = {
  name: 'appStorageDeploy'
  params: {
    location: resourceGroupLocation
    environmentName: environmentName
  }
}

/*
  SERVICE BUS
*/

module serviceBus 'serviceBus.bicep' = {
  name: 'serviceBusDeploy'
  params: {
    environmentName: environmentName
    location: resourceGroupLocation
  }
}

/*
  FUNCTION APP
*/

module functionApp 'functionApp.bicep' = {
  name: 'functionAppDeploy'
  params: {
    location: resourceGroupLocation
    environmentName: environmentName
    appStorageAccountName: appStorage.outputs.name
    serviceBusName: serviceBus.outputs.namespaceName
  }
}

output webAppName string = functionApp.outputs.functionAppName
