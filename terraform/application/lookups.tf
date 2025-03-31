data "azuread_client_config" "current" {}

#begin: resource groups
data "azurerm_resource_group" "networking_resource_group" {
  name = "RG-${local.product_name}-connectivity"
}

data "azurerm_resource_group" "terraform_resource_group" {
  name = "rg-${local.product_name}-terraform"
}

data "azurerm_resource_group" "build_agent_resource_group" {
  name = "rg-${local.product_name}-build-agents"
}

data "azurerm_resource_group" "analytics_resource_group" {
  name = "rg-${local.product_name}-analytics"
}
#end: resource groups

# begin: ddei lookup
#data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
#  name                = local.ddei_resource_name
#  resource_group_name = "rg-${local.ddei_resource_name}"
#}
# end: ddei lookup

#begin: vNET and route table lookups
data "azurerm_virtual_network" "complex_cases_vnet" {
  name                = "vnet-${local.product_name}-WANNET"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_route_table" "complex_cases_rt" {
  name                = "RT-${local.product_name}"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_network_security_group" "complex_cases_nsg" {
  name                = var.nsg_name
  resource_group_name = data.azurerm_resource_group.build_agent_resource_group.name
}
#end: vNET lookup

# begin: build agent subnet lookup

data "azurerm_subnet" "build_agent_subnet" {
  name                 = "LaCC-scale-set-subnet"
  resource_group_name  = data.azurerm_resource_group.build_agent_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
}

# end: build agent subnet lookup

# begin: vnet dns zone lookups
data "azurerm_private_dns_zone" "dns_zone_blob_storage" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_private_dns_zone" "dns_zone_table_storage" {
  name                = "privatelink.table.core.windows.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_private_dns_zone" "dns_zone_file_storage" {
  name                = "privatelink.file.core.windows.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_private_dns_zone" "dns_zone_queue_storage" {
  name                = "privatelink.queue.core.windows.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_private_dns_zone" "dns_zone_apps" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_private_dns_zone" "dns_zone_keyvault" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

# end: vnet dns zone lookups

# begin: app insights lookups
data "azurerm_application_insights" "complex_cases_ai" {
  name                = "${local.product_name}-${local.shared_prefix}-ai"
  resource_group_name = data.azurerm_resource_group.analytics_resource_group.name
}
