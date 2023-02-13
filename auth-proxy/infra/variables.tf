variable "environment" {
  type = string
  default = "dev"
}

variable "github_secret" {
  type = string
  sensitive = true
  default = ""
}

variable "github_client_id" {
  type = string
  sensitive = true
  default = ""
}

variable "env0_environment" {
  type    = string
  default = "dev"
}

