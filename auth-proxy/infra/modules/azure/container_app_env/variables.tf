variable "name" {
  type = string
}

variable "location" {
  type = string
  default = "West Europe"
}

variable "resource_group_id" {
  description = "(Required) The resource id of the resource group in which to create the resource. Changing this forces a new resource to be created."
  type        = string
}

variable "logs_workspace_id" {
  type = string
  default = null
}

variable "logs_workspace_key" {
  type = string
  default = null
}

variable "zone_redundant" {
  type = bool
  default = false
}

variable "subnet" {
  type = string
  default = null
}

variable "internal" {
  type = bool
  default = false
}

variable "dapr_instrumentation_key" {
  type = string
  default = null
}

variable "tags" {
  description = "(Optional) Specifies the tags of the log analytics workspace"
  type        = map(any)
  default     = {}
}

// Azure Files integration
variable "azurefile_name" {
  type = string
  default = null
}

variable "azurefile_account_name" {
  type = string
  default = null
}

variable "azurefile_account_key" {
  type = string
  default = null
}

variable "azurefile_share_name" {
  type = string
  default = null
}