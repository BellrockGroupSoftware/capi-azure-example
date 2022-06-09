@description('Environment type being deployed')
@allowed([
  'Dev'
  'Test'
  'Live'
])
param environmentName string

@description('Location for all resources.')
param location string = resourceGroup().location
param appStorageAccountName string
param serviceBusName string

var sharedNamePrefixes = json(loadTextContent('./shared-prefixes.json'))

var appInsightsName = toLower('${sharedNamePrefixes.applicationInsights}-capi-${environmentName}-${uniqueString(resourceGroup().id)}')
var appPlanName = toLower('${sharedNamePrefixes.appService}-capi-${environmentName}-${uniqueString(resourceGroup().id)}')
var functionAppName = toLower('${sharedNamePrefixes.functionApp}-capi-${environmentName}-${uniqueString(resourceGroup().id)}')

resource appStorage 'Microsoft.Storage/storageAccounts@2021-08-01' existing = {
  name: appStorageAccountName
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' existing = {
  name: serviceBusName
}

/*
  APP INSIGHTS
*/
resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  tags: {
    // circular dependency means we can't reference functionApp directly  /subscriptions/<subscriptionId>/resourceGroups/<rg-name>/providers/Microsoft.Web/sites/<appName>"
    'hidden-link:/subscriptions/${subscription().id}/resourceGroups/${resourceGroup().name}/providers/Microsoft.Web/sites/${functionAppName}': 'Resource'
  }
}

/*
  APP PLAN
*/
resource appPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appPlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
    family: 'Y'
    capacity: 0
  }
}

/*
  FUNCTION APP
*/
resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  tags: {
    project: 'CAPI'
  }
  kind: 'functionapp'
  properties: {
    serverFarmId: appPlan.id
    siteConfig: {
      appSettings: [
        {
          'name': 'APPINSIGHTS_INSTRUMENTATIONKEY'
          'value': appInsights.properties.InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${appStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(appStorage.id, appStorage.apiVersion).keys[0].value}'
        }
        {
          'name': 'FUNCTIONS_EXTENSION_VERSION'
          'value': '~4'
        }
        {
          'name': 'FUNCTIONS_WORKER_RUNTIME'
          'value': 'dotnet'
        }
        {
          'name': 'WEBSITE_RUN_FROM_PACKAGE'
          'value': '1'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${appStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(appStorage.id, appStorage.apiVersion).keys[0].value}'
        }
        {
          name: 'sas-capi-read-write'
          value: listkeys('${serviceBus.id}/AuthorizationRules/sas-capi-read-write', serviceBus.apiVersion).primaryConnectionString
        }
        {
          name: 'capi-webhook-secret'
          value: ''
        }
        {
          name: 'capi-api-key'
          value: ''
        }
        {
          name: 'capi-api-secret'
          value: ''
        }
        {
          name: 'capi-base-path'
          value: ''
        }
      ]
    }
  }
}

output functionAppName string = functionApp.name
output appPlanName string = appPlan.name
