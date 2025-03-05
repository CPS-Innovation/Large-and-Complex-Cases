resource "azurerm_network_security_group" "complex_cases_nsg" {
  location            = data.azurerm_virtual_network.complex_cases_vnet.location
  name                = "${local.product_name}-nsg"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}