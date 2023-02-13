terraform {
  required_providers {
    azapi = {
      source  = "bcc-code/azapi"
      version = "~> 1.2.1"
    }
  }
}

resource "azapi_resource" "container_app" {
  name      = var.container_app_name
  location  = var.location
  parent_id = var.resource_group_id
  type      = "Microsoft.App/containerApps@2022-03-01"
  tags      = var.tags

  identity {
    type = var.identity.type
    identity_ids = var.identity.identity_ids
  }

  body = jsonencode({
    properties = {

      managedEnvironmentId = var.managed_environment_id
      configuration = {
        ingress = {
          external   = true
          targetPort = 8080
        }
      }
      template = {
        containers = [{
          image = "hello-world:latest"
          name  = "temp"
          resources = {
            cpu    = 0.5
            memory = "1Gi"
          }
        }]
        scale = {
          minReplicas = 0
          maxReplicas = 1
        }
      }

    }
  })

  response_export_values = ["properties.configuration.ingress.fqdn"]
  lifecycle {
    ignore_changes = [
      body
    ]
  }
}
