resource "azurerm_storage_account" "sacpsccui" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccui"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location

  account_kind                    = "StorageV2"
  account_replication_type        = "RAGRS"
  account_tier                    = "Standard"
  min_tls_version                 = "TLS1_2"
  public_network_access_enabled   = true
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = true

  network_rules {
    default_action = "Deny"
    bypass         = ["Metrics", "Logging", "AzureServices"]
    virtual_network_subnet_ids = [
      azurerm_subnet.sn_complex_cases_ui_subnet.id,
      azurerm_subnet.sn_complex_cases_endpoints_subnet.id,
      data.azurerm_subnet.build_agent_subnet.id
    ]
  }

  sas_policy {
    expiration_period = "0.0:05:00"
  }

  blob_properties {
    delete_retention_policy {
      days = 7
    }
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags

  depends_on = [azurerm_subnet.sn_complex_cases_storage_subnet]
}

resource "azurerm_storage_account_queue_properties" "sacpsccui_queue_properties" {
  storage_account_id = azurerm_storage_account.sacpsccui.id
  logging {
    delete                = true
    read                  = true
    write                 = true
    version               = "1.0"
    retention_policy_days = 7
  }

  hour_metrics {
    include_apis          = true
    version               = "1.0"
    retention_policy_days = 7
  }

  minute_metrics {
    include_apis          = true
    version               = "1.0"
    retention_policy_days = 7
  }

  depends_on = [azurerm_storage_account.sacpsccui]
}

resource "azurerm_private_endpoint" "sacpsccui_blob_pe" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccui-blob-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = azurerm_subnet.sn_complex_cases_storage_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "complex-cases-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_blob_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccui-blob-psc"
    private_connection_resource_id = azurerm_storage_account.sacpsccui.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}

resource "azurerm_private_endpoint" "sacpsccui_table_pe" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccui-table-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = azurerm_subnet.sn_complex_cases_storage_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "complex-cases-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_table_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccui-table-psc"
    private_connection_resource_id = azurerm_storage_account.sacpsccui.id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }
}

resource "azurerm_private_endpoint" "sacpsccui_file_pe" {
  name                = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccui-file-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = azurerm_subnet.sn_complex_cases_storage_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "complex-cases-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_file_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.environment.alias != "prod" ? var.environment.alias : ""}ccui-file-psc"
    private_connection_resource_id = azurerm_storage_account.sacpsccui.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }
}

resource "azapi_resource" "sacpsccui_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "ui-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_complex_cases.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpsccui.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpsccui]
}
