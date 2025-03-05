data "azuread_client_config" "current" {}

data "azurerm_resource_group" "networking_resource_group" {
  name = "RG-${local.product_name}-connectivity"
}

# begin: ddei lookup
#data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
#  name                = local.ddei_resource_name
#  resource_group_name = "rg-${local.ddei_resource_name}"
#}
# end: ddei lookup

#begin: vNET lookup
data "azurerm_virtual_network" "complex_cases_vnet" {
  name                = "vnet-${local.product_name}-WANNET"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}
#end: vNET lookup

#begin: vnet subnet lookups
data "azurerm_subnet" "complex_cases_storage_subnet" {
  name                 = "${local.product_prefix}-storage-subnet"
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_subnet" "complex_cases_ui_subnet" {
  name                 = "${local.product_prefix}-ui-subnet"
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_subnet" "complex_cases_api_subnet" {
  name                 = "${local.product_prefix}-api-subnet"
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_subnet" "complex_cases_endpoints_subnet" {
  name                 = "${local.product_prefix}-endpoints-subnet"
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
}

#end: vnet subnet lookups

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

data "azurerm_private_dns_zone" "dns_zone_cognitive_account" {
  name                = "privatelink.cognitiveservices.azure.com"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_private_dns_zone" "dns_zone_search_service" {
  name                = "privatelink.search.windows.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}
# end: vnet dns zone lookups

# begin: app insights lookups
data "azurerm_application_insights" "complex_cases_ai" {
  name                = "${local.product_prefix}-ai"
  resource_group_name = "rg-${local.product_prefix}-analytics"
}

data "azurerm_log_analytics_workspace" "complex_cases_la" {
  name                = "${local.product_prefix}-la"
  resource_group_name = "rg-${local.product_prefix}-analytics"
}
# end: app insights lookups

data "azurerm_key_vault" "terraform_key_vault" {
  name                = "${local.product_name}kv${local.shared_suffix}terraform"
  resource_group_name = "rg-${local.product_name}terraform"
}