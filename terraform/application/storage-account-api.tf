resource "azurerm_storage_account" "sacpsccapi" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccapi"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location

  account_kind                    = "StorageV2"
  account_replication_type        = "RAGRS"
  account_tier                    = "Standard"
  min_tls_version                 = "TLS1_2"
  public_network_access_enabled   = false
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = true

  network_rules {
    default_action = "Deny"
    bypass         = ["Metrics", "Logging", "AzureServices"]
    virtual_network_subnet_ids = [

    ]
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags
}

resource "azurerm_private_endpoint" "sacpsccapi_blob_pe" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccapi-blob-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_storage_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "complex-cases-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_blob_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccapi-blob-psc"
    private_connection_resource_id = azurerm_storage_account.sacpsccapi.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}

resource "azurerm_private_endpoint" "sacpsccapi_table_pe" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccapi-table-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_storage_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "complex-cases-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_table_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccapi-table-psc"
    private_connection_resource_id = azurerm_storage_account.sacpsccapi.id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }
}

resource "azurerm_private_endpoint" "sacpsccapi_file_pe" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccapi-file-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_storage_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "complex-cases-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_file_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccapi-file-psc"
    private_connection_resource_id = azurerm_storage_account.sacpsccapi.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }
}

resource "azapi_resource" "sacpsccapi_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "ccapi-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_complex_cases.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpsccapi.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpsccapi]
}

resource "azapi_resource" "sacpsccapi_staging_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "ccapi-content-share-1"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_complex_cases.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpsccapi.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpsccapi]
}