resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_blob_storage_link" {
  name                  = "dnszonelink-blobstorage"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_blob_storage.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_blob_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_table_storage_link" {
  name                  = "dnszonelink-tablestorage"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_table_storage.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_table_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_file_storage_link" {
  name                  = "dnszonelink-filestorage"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_file_storage.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_file_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_apps_link" {
  name                  = "dnszonelink-apps"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_apps.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_apps]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_queue_link" {
  name                  = "dnszonelink-queue"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_queue_storage.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_queue_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_keyvault_link" {
  name                  = "dnszonelink-keyvault"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_key_vault.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_key_vault]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_monitor_link" {
  name                  = "dnszonelink-monitor"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_monitor.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_monitor]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_oms_link" {
  name                  = "dnszonelink-oms"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_oms.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_oms]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_ods_link" {
  name                  = "dnszonelink-ods"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_ods.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_ods]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_agentsvc_link" {
  name                  = "dnszonelink-agentsvc"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_agentsvc.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_agentsvc]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_service_bus_link" {
  name                  = "dnszonelink-service-bus"
  resource_group_name   = data.azurerm_resource_group.networking_resource_group.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_service_bus.name
  virtual_network_id    = data.azurerm_virtual_network.complex_cases_vnet.id
  tags                  = local.common_tags

  depends_on = [azurerm_private_dns_zone.dns_zone_service_bus]
}