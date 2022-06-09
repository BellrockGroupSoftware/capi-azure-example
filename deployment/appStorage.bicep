@description('Environment type being deployed')
@allowed([
  'Dev'
  'Test'
  'Live'
])
param environmentName string

@description('Location for all resources.')
param location string = resourceGroup().location

var sharedNamePrefixes = json(loadTextContent('./shared-prefixes.json'))

var storageAccountName = toLower('${sharedNamePrefixes.storageAccount}${environmentName}app${uniqueString(resourceGroup().id)}')

resource appStorage 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

output id string = appStorage.id
output name string = appStorage.name
output apiVersion string = appStorage.apiVersion
