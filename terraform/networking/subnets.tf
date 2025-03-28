resource "azurerm_subnet" "sn_complex_cases_ampls_subnet" {
  name                 = "${local.product_name}-ampls-subnet"
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  address_prefixes     = var.subnets.ampls

  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true
}

resource "azurerm_subnet_route_table_association" "rt_assocation_complex_cases_ampls_subnet" {
  route_table_id = data.azurerm_route_table.complex_cases_rt.id
  subnet_id      = azurerm_subnet.sn_complex_cases_ampls_subnet.id
  depends_on     = [azurerm_subnet.sn_complex_cases_ampls_subnet]
}

resource "azurerm_subnet_network_security_group_association" "nsg_association_complex_cases_ampls_subnet" {
  network_security_group_id = data.azurerm_network_security_group.complex_cases_nsg.id
  subnet_id                 = azurerm_subnet.sn_complex_cases_ampls_subnet.id
  depends_on                = [azurerm_subnet.sn_complex_cases_ampls_subnet]
}
