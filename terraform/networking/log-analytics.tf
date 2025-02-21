resource "azurerm_log_analytics_workspace" "complex_cases_la" {
  name                       = "${local.product_name}-la"
  location                   = azurerm_resource_group.rg_polaris_workspace.location
  resource_group_name        = azurerm_resource_group.rg_polaris_workspace.name
  sku                        = "PerGB2018"
  retention_in_days          = var.appinsights_configuration.log_retention_days
  internet_ingestion_enabled = var.appinsights_configuration.analytics_internet_ingestion_enabled
  internet_query_enabled     = var.appinsights_configuration.analytics_internet_query_enabled
  tags                       = local.common_tags
}
