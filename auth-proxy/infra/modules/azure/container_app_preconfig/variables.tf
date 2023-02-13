variable "managed_environment_id" {
  description = "(Required) Specifies the id of the managed environment."
  type        = string
}

variable "resource_group_id" {
  description = "(Required) The resource id of the resource group in which to create the resource. Changing this forces a new resource to be created."
  type        = string
}

variable "location" {
  description = "(Optional) Specifies the supported Azure location where the resource should be created. Changing this forces a new resource to be created."
  type        = string
  default     = "West Europe"
}

variable "identity" {
  description = "(Optional) Specifies what identities should the Container App posess."
  type = object({
    type   = string
    identity_ids = optional(list(string))
  })
  default = null

  validation {
    condition = contains(["SystemAssigned", "SystemAssigned, UserAssigned", "UserAssigned"], var.identity.type)
    error_message = "Supported values for identity type can only be \"SystemAssigned,UserAssigned\", \"SystemAssigned\" or \"UserAssigned\"."
  }
}

variable "container_app_name" {
  type = string
}

variable "tags" {
  description = "(Optional) Specifies the tags of the log analytics workspace"
  type        = map(any)
  default     = {}
}