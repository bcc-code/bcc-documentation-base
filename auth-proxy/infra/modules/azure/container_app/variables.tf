variable "managed_environment_id" {
  description = "(Required) Specifies the id of the managed environment."
  type        = string
}

variable "container_app_preconfig_id" {
  description = "(Required) Specifies the id of the preconfigured Container App which will be updated."
  type        = string
}

variable "container_app" {
  description = "Specifies the container apps in the managed environment."
  type = object({
    configuration = object({
      ingress = optional(object({
        external   = optional(bool, true)
        targetPort = optional(number, 80)
      }))
      dapr = optional(object({
        enabled     = optional(bool)
        appId       = optional(string)
        appProtocol = optional(string)
        appPort     = optional(number)
      }))
      secrets = optional(list(object({
        name  = string
        value = string
      })), [])
      registries = optional(list(object({
        server            = string
        username          = optional(string)
        passwordSecretRef = optional(string)
        identity          = optional(string)
      })), [])
    })
    template = object({
      containers = list(object({
        image = optional(string, "hello-world:latest")
        name  = optional(string, "hello-world")
        env = optional(list(object({
          name      = string
          value     = optional(string)
          secretRef = optional(string)
        })))
        resources = optional(object({
          cpu    = optional(number, 0.25)
          memory = optional(string, "0.1Gi")
        }))
        volumeMounts = optional(list(object({
          mountPath  = string
          volumeName = string
        })))
      }))
      scale = optional(object({
        minReplicas = optional(number)
        maxReplicas = optional(number)
      }))
      volumes = optional(list(object({
        name        = string
        storageType = string
        storageName = string
      })))
    })
  })
}

variable "dapr_components" {
  description = "Specifies the dapr components in the managed environment."
  type = list(object({
    name          = string
    componentType = string
    version       = string
    ignoreErrors  = optional(bool)
    initTimeout   = string
    secrets = optional(list(object({
      name  = string
      value = any
    })))
    metadata = optional(list(object({
      name      = string
      value     = optional(any)
      secretRef = optional(any)
    })))
    scopes = optional(list(string))
  }))
  default = []
}
