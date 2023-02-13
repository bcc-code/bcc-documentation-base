terraform {
  required_providers {
    azapi = {
      source  = "bcc-code/azapi"
      version = "~> 1.2.1"
    }
    azurerm = {
      source = "hashicorp/azurerm"
      version = ">= 3.38.0"
    }
  }
}

resource "azapi_resource" "managed_environment" {
  name      = var.name
  location  = var.location
  parent_id = var.resource_group_id
  type      = "Microsoft.App/managedEnvironments@2022-03-01"
  tags      = var.tags
  
  body = jsonencode({
    properties = {
      daprAIInstrumentationKey = var.dapr_instrumentation_key
      appLogsConfiguration = {
        destination = "log-analytics"
        logAnalyticsConfiguration = {
          customerId=var.logs_workspace_id
          sharedKey=var.logs_workspace_key
        }
      }
      vnetConfiguration = {
        infrastructureSubnetId = var.subnet
        internal = var.internal
      }
      zoneRedundant = var.zone_redundant
    }
  })

  lifecycle {
    ignore_changes = [
        tags
    ]
  }
}

resource "azapi_resource" "managed_environment_azurefile" {
  count = var.azurefile_name != null ? 1 : 0
  name      = var.azurefile_name
  parent_id = azapi_resource.managed_environment.id
  type      = "Microsoft.App/managedEnvironments/storages@2022-03-01"
  body = jsonencode({
    properties = {
      azureFile = {
          accountName = var.azurefile_account_name
          accountKey = var.azurefile_account_key
          shareName = var.azurefile_share_name
          accessMode = "ReadWrite"
        }
    }})
}

# TODO merge default obj with user provided one
# TODO fix shared container registry data block, to be replaced with cr_domain or smthing like taht