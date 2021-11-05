var location = resourceGroup().location

resource servicebus 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: 'mfsbdemo'
  sku: {
    name:'Standard'
  }
  location:'uksouth'
}

resource myqueue 'Microsoft.ServiceBus/namespaces/queues@2021-06-01-preview' = {
  name: 'myqueue'
  parent: servicebus
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: false
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 1
    requiresDuplicateDetection: false
    requiresSession: false
  }
}

resource queuefunauthrule 'Microsoft.ServiceBus/namespaces/queues/authorizationRules@2021-06-01-preview' = {
  name: 'funcauthrule'
  parent: myqueue
  properties: {
    rights: [ 
      'Manage'
      'Listen'
      'Send'
    ]
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: 'mfsbfuncstore'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource serverFarm 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: 'mfsbappplan'
  location: location
  sku: {
    name: 'EP1'
    tier: 'ElasticPremium'
  }
  kind: 'elastic'
  properties: {
    maximumElasticWorkerCount: 20
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'mfsbappinsights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource function 'Microsoft.Web/sites@2020-06-01' = {
  name: 'mfsbfunc'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: serverFarm.id
    siteConfig: {
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsights.properties.InstrumentationKey}'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix= ${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value};'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~12'
        }
        {
          name: 'demo1_connection_string'
          value: 'Endpoint=sb://${servicebus.name}.servicebus.windows.net/;SharedAccessKeyName=${queuefunauthrule.name};SharedAccessKey=${listKeys(queuefunauthrule.id, servicebus.apiVersion).primaryKey}'
        }
        {
          name: 'LockWait'
          value: '60000'
        }
      ]
    }
  }
}

