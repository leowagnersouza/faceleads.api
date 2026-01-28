targetScope = 'subscription'

@description('Name of the resource group to create')
param rgName string = 'rg-faceleads-dev'

@description('Azure region for the resource group')
param location string = 'brazilsouth'

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: location
}

output resourceGroupId string = rg.id
output resourceGroupName string = rg.name
output resourceGroupLocation string = rg.location
