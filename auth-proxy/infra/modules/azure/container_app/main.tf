terraform {
  required_providers {
    azapi = {
      source  = "bcc-code/azapi"
      version = "~> 1.2.1"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.38.0"
    }
  }
}

resource "azapi_resource" "daprComponents" {
  for_each = { for component in var.dapr_components : component.name => component }

  name      = each.key
  parent_id = var.managed_environment_id
  type      = "Microsoft.App/managedEnvironments/daprComponents@2022-03-01"

  body = jsonencode({
    properties = {
      componentType = each.value.componentType
      version       = each.value.version
      ignoreErrors  = each.value.ignoreErrors
      initTimeout   = each.value.initTimeout
      secrets       = each.value.secrets
      metadata      = each.value.metadata
      scopes        = each.value.scopes
    }
  })
}

#ref
# https://raw.githubusercontent.com/Azure/azure-resource-manager-schemas/68af7da6820cc91660904b34813aeee606c400f1/schemas/2022-03-01/Microsoft.App.json

resource "azapi_update_resource" "container_app" {
  resource_id = var.container_app_preconfig_id
  type        = "Microsoft.App/containerApps@2022-03-01"

  body = jsonencode({
    properties = {

      managedEnvironmentId = var.managed_environment_id
      configuration = {
        registries = var.container_app.configuration.registries
        secrets = var.container_app.configuration.secrets
        ingress = var.container_app.configuration.ingress
        dapr    = var.container_app.configuration.dapr
      }
      template = var.container_app.template

    }
  })

  response_export_values = ["properties.configuration.ingress.fqdn"]
}
