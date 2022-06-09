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
var serviceBusName = toLower('${sharedNamePrefixes.serviceBus}-capi-${environmentName}-${uniqueString(resourceGroup().id)}')

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: serviceBusName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    disableLocalAuth: false
    zoneRedundant: false
  }
}

resource sasIotReadWrite 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2021-06-01-preview' = {
  parent: serviceBusNamespace
  name: 'sas-capi-read-write'
  properties: {
    rights: [
      'Listen'
      'Send'
    ]
  }
}

var queues = [
  {
    name: 'pending-order-messages'
  }
  {
    name: 'enriched-messages'
  }
]

module serviceBusQueue 'serviceBusQueue.bicep' = [for queue in queues: {
  name: '${sharedNamePrefixes.serviceBusQueue}-${queue.name}-deploy'
  params: {
    serviceBusNamespaceName: serviceBusNamespace.name
    queueName: '${queue.name}'
  }
}]

var distributeWebhookEvents = toLower('${sharedNamePrefixes.serviceBusTopic}-distribute-webhook-events')

// Creates a topic for distributing webhook events
resource distributeWebhookEventsTopic 'Microsoft.ServiceBus/namespaces/topics@2021-11-01' = {
  parent: serviceBusNamespace
  name: distributeWebhookEvents
  resource orderSubscription 'subscriptions' = {
    name: 'event-type-order'
    properties: {
      forwardTo: '${sharedNamePrefixes.serviceBusQueue}-pending-order-messages'
    }
    resource orderEventsRule 'rules' = {
      name: 'OrderEvent'
      properties: {
        action: {}
        filterType: 'CorrelationFilter'
        correlationFilter: {
          properties: {
            eventType: 'order'
          }
        }
      }
    }
  }
}

var distributeChannelEvents = toLower('${sharedNamePrefixes.serviceBusTopic}-distribute-channel-events')

// Creates a topic for distributing webhook events
resource distributeChannelEventsTopic 'Microsoft.ServiceBus/namespaces/topics@2021-11-01' = {
  parent: serviceBusNamespace
  name: distributeChannelEvents
  resource allChannelsSubscription 'subscriptions' = {
    name: 'all-channels'
    properties: {
      forwardTo: '${sharedNamePrefixes.serviceBusQueue}-enriched-messages'
    }
    resource allReadingsRule 'rules' = {
      name: 'All'
      properties: {
        action: {}
        filterType: 'SqlFilter'
        sqlFilter: {
          sqlExpression: '1=1'
          compatibilityLevel: 20
        }
      }
    }
  }
}

output namespaceName string = serviceBusNamespace.name
