resource "azurerm_subnet" "sn_complex_cases_storage_subnet" {
  name                 = "${local.product_prefix}-storage-subnet"
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  address_prefixes     = var.subnets.storage
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault"]

  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true
}

resource "azurerm_subnet_route_table_association" "rt_assocation_complex_cases_storage_subnet" {
  route_table_id = data.azurerm_route_table.complex_cases_rt.id
  subnet_id      = azurerm_subnet.sn_complex_cases_storage_subnet.id
  depends_on     = [azurerm_subnet.sn_complex_cases_storage_subnet]
}

resource "azurerm_subnet_network_security_group_association" "nsg_association_complex_cases_storage_subnet" {
  network_security_group_id = data.azurerm_network_security_group.complex_cases_nsg.id
  subnet_id                 = azurerm_subnet.sn_complex_cases_storage_subnet.id
  depends_on                = [azurerm_subnet.sn_complex_cases_storage_subnet]
}

##############

resource "azurerm_subnet" "sn_complex_cases_ui_subnet" {
  name                 = "${local.product_prefix}-ui-subnet"
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  address_prefixes     = var.subnets.ui
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.Web"]

  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "Microsoft.Web/serverFarms UI Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet_route_table_association" "rt_assocation_complex_cases_ui_subnet" {
  route_table_id = data.azurerm_route_table.complex_cases_rt.id
  subnet_id      = azurerm_subnet.sn_complex_cases_ui_subnet.id
  depends_on     = [azurerm_subnet.sn_complex_cases_ui_subnet]
}

resource "azurerm_subnet_network_security_group_association" "nsg_association_complex_cases_ui_subnet" {
  network_security_group_id = data.azurerm_network_security_group.complex_cases_nsg.id
  subnet_id                 = azurerm_subnet.sn_complex_cases_ui_subnet.id
  depends_on                = [azurerm_subnet.sn_complex_cases_ui_subnet]
}

##############

resource "azurerm_subnet" "sn_complex_cases_api_subnet" {
  name                 = "${local.product_prefix}-api-subnet"
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  address_prefixes     = var.subnets.api
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.Web"]

  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "Microsoft.Web/serverFarms API Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet_route_table_association" "rt_assocation_complex_cases_api_subnet" {
  route_table_id = data.azurerm_route_table.complex_cases_rt.id
  subnet_id      = azurerm_subnet.sn_complex_cases_api_subnet.id
  depends_on     = [azurerm_subnet.sn_complex_cases_api_subnet]
}

resource "azurerm_subnet_network_security_group_association" "nsg_association_complex_cases_api_subnet" {
  network_security_group_id = data.azurerm_network_security_group.complex_cases_nsg.id
  subnet_id                 = azurerm_subnet.sn_complex_cases_api_subnet.id
  depends_on                = [azurerm_subnet.sn_complex_cases_api_subnet]
}

##############

resource "azurerm_subnet" "sn_complex_cases_endpoints_subnet" {
  name                 = "${local.product_prefix}-endpoints-subnet"
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  address_prefixes     = var.subnets.endpoints
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.Web"]

  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true
}

resource "azurerm_subnet_route_table_association" "rt_assocation_complex_cases_endpoints_subnet" {
  route_table_id = data.azurerm_route_table.complex_cases_rt.id
  subnet_id      = azurerm_subnet.sn_complex_cases_endpoints_subnet.id
  depends_on     = [azurerm_subnet.sn_complex_cases_endpoints_subnet]
}

resource "azurerm_subnet_network_security_group_association" "nsg_association_complex_cases_endpoints_subnet" {
  network_security_group_id = data.azurerm_network_security_group.complex_cases_nsg.id
  subnet_id                 = azurerm_subnet.sn_complex_cases_endpoints_subnet.id
  depends_on                = [azurerm_subnet.sn_complex_cases_endpoints_subnet]
}

##############

resource "azurerm_subnet" "sn_complex_cases_egressMock_subnet" {
  name                 = "${local.product_prefix}-egressMock-subnet"
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  address_prefixes     = var.subnets.egressMock
  service_endpoints    = ["Microsoft.Storage"]

  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "Microsoft.Web/serverFarms Egress Mock Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet_route_table_association" "rt_assocation_complex_cases_egress_mock_subnet" {
  route_table_id = data.azurerm_route_table.complex_cases_rt.id
  subnet_id      = azurerm_subnet.sn_complex_cases_egressMock_subnet.id
  depends_on     = [azurerm_subnet.sn_complex_cases_egressMock_subnet]
}

resource "azurerm_subnet_network_security_group_association" "nsg_association_complex_cases_egress_mock_subnet" {
  network_security_group_id = data.azurerm_network_security_group.complex_cases_nsg.id
  subnet_id                 = azurerm_subnet.sn_complex_cases_egressMock_subnet.id
  depends_on                = [azurerm_subnet.sn_complex_cases_egressMock_subnet]
}

##############

resource "azurerm_subnet" "sn_complex_cases_netAppMock_subnet" {
  name                 = "${local.product_prefix}-netAppMock-subnet"
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
  virtual_network_name = data.azurerm_virtual_network.complex_cases_vnet.name
  address_prefixes     = var.subnets.netAppMock
  service_endpoints    = ["Microsoft.Storage"]

  private_endpoint_network_policies             = "Enabled"
  private_link_service_network_policies_enabled = true

  delegation {
    name = "Microsoft.Web/serverFarms NetApp Mock Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet_route_table_association" "rt_assocation_complex_cases_netapp_mock_subnet" {
  route_table_id = data.azurerm_route_table.complex_cases_rt.id
  subnet_id      = azurerm_subnet.sn_complex_cases_netAppMock_subnet.id
  depends_on     = [azurerm_subnet.sn_complex_cases_netAppMock_subnet]
}

resource "azurerm_subnet_network_security_group_association" "nsg_association_complex_cases_netapp_mock_subnet" {
  network_security_group_id = data.azurerm_network_security_group.complex_cases_nsg.id
  subnet_id                 = azurerm_subnet.sn_complex_cases_netAppMock_subnet.id
  depends_on                = [azurerm_subnet.sn_complex_cases_netAppMock_subnet]
}
