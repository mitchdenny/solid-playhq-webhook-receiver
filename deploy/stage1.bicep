param instanceName string = resourceGroup().name
param instanceLocation string = resourceGroup().location

resource acrPullRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
  scope: subscription()
}

resource acrPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: 'pullid${uniqueString(instanceName)}'
  location: instanceLocation
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' = {
  name: 'registry${uniqueString(instanceName)}'
  location: instanceLocation
  tags: {
    instanceName: instanceName
  }
  sku: {
    name: 'Basic'
  }
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, uniqueString(instanceName), acrPullRoleDefinition.id)
  properties: {
    principalId: acrPullIdentity.properties.principalId
    roleDefinitionId: acrPullRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
  scope: containerRegistry
}
