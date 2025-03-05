resource "azurerm_private_dns_resolver" "complex_cases_dns_resolver" {
  name                = "${local.product_name}-dns-resolver"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
  location            = data.azurerm_resource_group.networking_resource_group.location
  virtual_network_id  = data.azurerm_virtual_network.complex_cases_vnet.id

  tags = local.common_tags
}