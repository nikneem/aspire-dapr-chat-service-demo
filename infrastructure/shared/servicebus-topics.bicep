param landingzoneEnvironment object
param topics array

resource serviceBus 'Microsoft.ServiceBus/namespaces@2023-01-01-preview' existing = {
  name: landingzoneEnvironment.serviceBus
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = [
  for queue in topics: {
    name: queue.name
    parent: serviceBus
    properties: {
      defaultMessageTimeToLive: 'P7D'
      enablePartitioning: false
      duplicateDetectionHistoryTimeWindow: empty(queue.duplicateDetectionTimeWindow)
        ? 'PT10M'
        : queue.duplicateDetectionTimeWindow
      requiresDuplicateDetection: !empty(queue.duplicateDetectionTimeWindow)
    }
  }
]
