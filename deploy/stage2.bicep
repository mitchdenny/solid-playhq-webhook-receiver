param instanceName string = resourceGroup().name
param instanceLocation string = resourceGroup().location
param containerImage string
param eventHubNamespaceCount int

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2021-09-01' existing = {
  name: 'registry${uniqueString(instanceName)}'
}

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: 'config${uniqueString(instanceName)}'
  location: instanceLocation
  tags: {
    instanceName: instanceName
  }
  sku: {
    name: 'Standard'
  }
  identity: {
    type: 'None'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: 'vault${uniqueString(instanceName)}'
  location: instanceLocation
  tags: {
    instanceName: instanceName
  }
  properties: {
    enableRbacAuthorization: true
    accessPolicies: [
    ]
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: subscription().tenantId
  }
}

resource keyVaultSecretsUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '4633458b-17de-408a-b874-0445c86b69e6'
  scope: subscription()
}

resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, uniqueString(instanceName), keyVaultSecretsUserRoleDefinition.id)
  properties: {
    principalId: containerApp.identity.principalId
    roleDefinitionId: keyVaultSecretsUserRoleDefinition.id
  }
  scope: keyVault
}

resource logWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: 'logs${uniqueString(instanceName)}'
  location: instanceLocation
  tags: {
    instanceName: instanceName
  }
  properties: {
   sku: {
    name: 'PerGB2018'
   }
  }
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: 'env${uniqueString(instanceName)}'
  location: instanceLocation
  tags: {
    instanceName: instanceName
  }
  properties: {
   appLogsConfiguration: {
    destination: 'log-analytics'
     logAnalyticsConfiguration: {
     customerId: logWorkspace.properties.customerId
     sharedKey: logWorkspace.listKeys().primarySharedKey
     }
   }
  }
}

resource acrPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: 'pullid${uniqueString(instanceName)}'
}

resource containerApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: 'app${uniqueString(instanceName)}'
  location: instanceLocation
  tags: {
    instanceName: instanceName
  }
  identity: {
    type: 'SystemAssigned,UserAssigned'
    userAssignedIdentities: {
      '${acrPullIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 3000
      }
      registries: [
        {
         identity: acrPullIdentity.id
         server: containerRegistry.properties.loginServer
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'container${uniqueString(instanceName)}'
          image: containerImage
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'CONFIG_URL'
              value: appConfig.properties.endpoint
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/liveness'
                port: 3000
              }
              initialDelaySeconds: 7
              periodSeconds: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/readiness'
                port: 3000
              }
              initialDelaySeconds: 30
              periodSeconds: 3
            }
          ]
        }
      ]
      scale: {
         minReplicas: 1
      }
    }
  }
}

resource appConfigDataReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '516239f1-63e1-4d78-a4de-a74fb236a071'
  scope: subscription()
}

resource appConfigDataReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, uniqueString(instanceName), appConfigDataReaderRoleDefinition.id)
  properties: {
    principalId: containerApp.identity.principalId
    roleDefinitionId: appConfigDataReaderRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
  scope: appConfig
}

resource eventHubsNamespace 'Microsoft.EventHub/namespaces@2022-01-01-preview' = [for i in range(0, eventHubNamespaceCount): {
  name: 'eventhub${uniqueString(instanceName, string(i))}'
  location: instanceLocation
  tags: {
    instanceName: instanceName
    stamp: string(i)
  }
  sku: {
    capacity: 1
    name: 'Basic'
    tier: 'Basic'
  }
}]

resource eventHubDataSenderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '2b629674-e913-4c01-ae53-ef4638d8f975'
  scope: subscription()
}

resource eventHubDataSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for i in range(0, eventHubNamespaceCount): {
  name: guid(resourceGroup().id, uniqueString(instanceName, string(i)), eventHubDataSenderRoleDefinition.id)
  properties: {
    principalId: containerApp.identity.principalId
    roleDefinitionId: eventHubDataSenderRoleDefinition.id
  }
  scope: eventHubsNamespace[i]
}]

