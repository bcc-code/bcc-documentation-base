terraform {
  required_version = ">= 1.3.6"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.30.0, != 3.44.0, != 3.44.1"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.15.0"
    }

    azapi = {
      source  = "Azure/azapi"
      version = ">= 1.0.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.4.3"
    }
  }

  backend "azurerm" {}

}

provider "azurerm" {
  # use_oidc = true
  features {}
}

provider "azapi" {
}

provider "azuread" {
  #tenant_id = "00000000-0000-0000-0000-000000000000"
}

data "azurerm_client_config" "current" {}

locals {
  location             = "West Europe"
  project_name         = "docsite"
  resource_group       = "${local.project_name}-${var.app_environment}"
  storage_account_name = replace(local.project_name, "-", "")
  tags                 = {}
  azurefile_name       = "pagebuilds"
  volume_name          = "azure-files-volume"
  shared_cr_fqdn       = "crbccplatform${var.platform_environment}.azurecr.io"
}

# Vault for OIDC JsonWebKey
resource "azurerm_key_vault" "keyvault" {
  name                        = lower(replace("${local.project_name}-${var.app_environment}-vault", "-", ""))
  location                    = data.azurerm_resource_group.main.location
  resource_group_name         = data.azurerm_resource_group.main.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  purge_protection_enabled    = true
  enabled_for_disk_encryption = true
  enable_rbac_authorization   = true
}

data "http" "myip" {
  url = "http://ipv4.icanhazip.com"
}

# Get project resource group
data "azurerm_resource_group" "main" {
  name = local.resource_group
}


# Analytics Workspace
module "log_analytics_workspace" {
  source              = "./modules/azure/log_analytics"
  name                = "${local.project_name}-logs"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  tags                = local.tags

}

# Application Insights
module "application_insights" {
  source              = "./modules/azure/application_insights"
  name                = "${local.project_name}-env-insights"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  tags                = local.tags
  application_type    = "web"
  workspace_id        = module.log_analytics_workspace.id
}


# # VLAN for Container Environment
# module "container_apps_vlan" {
#   source                           = "./modules/azure/container_apps_vlan"
#   name                             = "${local.project_name}-vlan"
#   location                         = data.azurerm_resource_group.main.location
#   resource_group_name              = data.azurerm_resource_group.main.name
#   tags                             = local.tags

#   depends_on = [
#     azurerm_resource_group.rg
#   ]
# }

# Storage Account
resource "azurerm_storage_account" "azurefiles" {
  name                            = "docsstorageaccount${var.app_environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  account_replication_type        = "LRS"
  allow_nested_items_to_be_public = false
  default_to_oauth_authentication = true
  account_tier                    = "Standard"
  blob_properties {
    change_feed_enabled           = true
    change_feed_retention_in_days = 7
    delete_retention_policy {
      days = 7
    }
  }
}

# Storage account file share
resource "azurerm_storage_share" "azurefile_share" {
  name                 = local.azurefile_name
  storage_account_name = azurerm_storage_account.azurefiles.name
  quota                = 10
}

data "azurerm_user_assigned_identity" "cr_reader" {
  name                = "id-cr-reader-${data.azurerm_resource_group.main.name}"
  resource_group_name = data.azurerm_resource_group.main.name
}

data "azurerm_user_assigned_identity" "db_owner" {
  name                = "id-db-owner-${data.azurerm_resource_group.main.name}"
  resource_group_name = data.azurerm_resource_group.main.name
}

module "api_container_app_preconfig" {
  source                 = "./modules/azure/container_app_preconfig"
  container_app_name     = "ca-docsite-api"
  managed_environment_id = module.container_app_env.id
  location               = data.azurerm_resource_group.main.location
  resource_group_id      = data.azurerm_resource_group.main.id
  identity = {
    type         = "SystemAssigned, UserAssigned"
    identity_ids = [data.azurerm_user_assigned_identity.cr_reader.id, data.azurerm_user_assigned_identity.db_owner.id]
  }
}
moved {
  from = module.api_container_app.azapi_resource.container_app
  to   = module.api_container_app_preconfig.azapi_resource.container_app
}
moved {
  from = module.container_apps_env.azapi_resource.managed_environment
  to   = module.container_app_env.azapi_resource.managed_environment
}
moved {
  from = module.container_apps_env.azapi_resource.managed_environment_azurefile
  to   = module.container_app_env.azapi_resource.managed_environment_azurefile[0]
}

#key vault role assignment
resource "azurerm_role_assignment" "arakey" {
  scope                            = azurerm_key_vault.keyvault.id
  role_definition_name             = "Key Vault Crypto User"
  principal_id                     = module.api_container_app_preconfig.identity[0].principal_id
  skip_service_principal_aad_check = true
}

#storage account role assignment
resource "azurerm_role_assignment" "arasto" {
  scope                            = azurerm_storage_account.azurefiles.id
  role_definition_name             = "Storage Blob Data Contributor"
  principal_id                     = module.api_container_app_preconfig.identity[0].principal_id
  skip_service_principal_aad_check = true
}

#Configure authentication to container app
resource "azapi_resource" "authconfig" {
  type      = "Microsoft.App/containerApps/authConfigs@2022-06-01-preview"
  name      = "current"
  parent_id = module.api_container_app_preconfig.id
  body = jsonencode({
    properties = {
      "platform" : {
        "enabled" : true
      },
      "globalValidation" : {
        "unauthenticatedClientAction" : "AllowAnonymous"
      },
      "identityProviders" : {
        "gitHub" : {
          "registration" : {
            "clientId" : var.github_client_id,
            "clientSecretSettingName" : "github-provider-authentication-secret"
          }
        }
      },
      "login" : {
        "preserveUrlFragmentsForLogins" : false,
        "allowedExternalRedirectUrls" : []
      }
    }
  })
  depends_on = [module.api_container_app]
}

# Container Environment
module "container_app_env" {
  source                   = "./modules/azure/container_app_env"
  name                     = "${local.project_name}-env"
  location                 = data.azurerm_resource_group.main.location
  resource_group_id        = data.azurerm_resource_group.main.id
  tags                     = local.tags
  dapr_instrumentation_key = module.application_insights.instrumentation_key
  logs_workspace_id        = module.log_analytics_workspace.workspace_id
  logs_workspace_key       = module.log_analytics_workspace.primary_shared_key
  azurefile_name           = local.azurefile_name
  azurefile_account_name   = azurerm_storage_account.azurefiles.name
  azurefile_account_key    = azurerm_storage_account.azurefiles.primary_access_key
  azurefile_share_name     = azurerm_storage_share.azurefile_share.name
  # vlan_subnet_id                   = module.container_apps_vlan.subnet_id
}

#ref:
# https://github.com/Azure/azure-resource-manager-schemas/blob/68af7da6820cc91660904b34813aeee606c400f1/schemas/2022-03-01/Microsoft.App.json

module "api_container_app" {
  source                     = "./modules/azure/container_app"
  managed_environment_id     = module.container_app_env.id
  container_app_preconfig_id = module.api_container_app_preconfig.id
  container_app = {
    configuration = {
      ingress = {
        external   = true
        targetPort = 8080
      }
      secrets = [
        {
          name  = "application-insights-connection-string"
          value = module.application_insights.connection_string
        },
        {
          name  = "github-provider-authentication-secret"
          value = var.github_secret
        }
      ]
      registries = [
        {
          server   = local.shared_cr_fqdn
          identity = data.azurerm_user_assigned_identity.cr_reader.id
        }
      ]
    }
    template = {
      containers = [
        {
          image = "${local.shared_cr_fqdn}/ca-docsite-api:latest"
          name  = "${local.project_name}-api"
          env = [
            {
              name  = "APP_PORT"
              value = 8080
            },
            {
              name  = "ASPNETCORE_URLS"
              value = "http://+:8080"
            },
            {
              name      = "APPLICATIONINSIGHTS_CONNECTION_STRING"
              secretRef = "application-insights-connection-string"
            },
            {
              name      = "APPLICATIONINSIGHTS__CONNECTIONSTRING"
              secretRef = "application-insights-connection-string"
            },
            {
              name  = "ENVIRONMENT_NAME"
              value = terraform.workspace
            }
          ],
          resources = {
            cpu    = 0.25
            memory = "0.5Gi"
          }
          volumeMounts = [{
            mountPath  = "/azfiles"
            volumeName = local.volume_name
          }]
        }
      ]
      scale = {
        minReplicas = 0
        maxReplicas = 10
        rules = [{
          name = "cpu"
          custom = {
            type = "cpu"
            metadata = {
              type  = "AverageValue"
              value = 60
            }
          }
        }]
      }
      volumes = [{
        name        = local.volume_name
        storageType = "AzureFile"
        storageName = local.azurefile_name
      }]
    }
  }
  depends_on = [module.container_app_env.azapi_resource]
}
