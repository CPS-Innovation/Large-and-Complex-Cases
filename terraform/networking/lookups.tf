data "azurerm_resource_group" "networking_resource_group" {
  name = "RG-${local.product_name}-connectivity"
}

data "azurerm_virtual_network" "complex_cases_vnet" {
  name                = "VNET-${local.product_name}-WANNET"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_route_table" "complex_cases_rt" {
  name                = "RT-${local.product_name}"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_key_vault" "terraform_key_vault" {
  name                = "${local.product_name}kv${local.shared_suffix}terraform"
  resource_group_name = "rg-${local.product_name}-terraform"
}
