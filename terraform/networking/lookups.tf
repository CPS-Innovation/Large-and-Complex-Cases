data "azurerm_resource_group" "networking_resource_group" {
  name = "RG-${local.product_name}-connectivity"
}

data "azurerm_resource_group" "terraform_resource_group" {
  name = "rg-${local.product_name}-terraform"
}

data "azurerm_resource_group" "build_agent_resource_group" {
  name = "rg-${local.product_name}-build-agents"
}

data "azurerm_virtual_network" "complex_cases_vnet" {
  name                = "VNET-${local.product_name}-WANNET"
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
