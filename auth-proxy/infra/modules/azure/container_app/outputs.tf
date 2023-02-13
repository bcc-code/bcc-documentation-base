output "domain_name" {
  value = jsondecode(azapi_update_resource.container_app.output).properties.configuration.ingress.fqdn
}

output "id" {
  value = azapi_update_resource.container_app.id
}