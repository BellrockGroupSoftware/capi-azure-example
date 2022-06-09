param queueName string
param serviceBusNamespaceName string

var sharedNamePrefixes = json(loadTextContent('./shared-prefixes.json'))

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: serviceBusNamespaceName
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2021-06-01-preview' = {
  parent: serviceBusNamespace
  name: '${sharedNamePrefixes.serviceBusQueue}-${queueName}'
}

output queueName string = serviceBusQueue.name
