resource "azurerm_application_insights" "complex_cases_ai" {
  name                       = "${local.product_name}-ai"
  location                   = azurerm_resource_group.rg_complex_cases_analytics.location
  resource_group_name        = azurerm_resource_group.rg_complex_cases_analytics.name
  workspace_id               = azurerm_log_analytics_workspace.complex_cases_la_workspace.id
  application_type           = "web"
  retention_in_days          = var.appinsights_configuration.log_retention_days
  tags                       = local.common_tags
  internet_ingestion_enabled = var.appinsights_configuration.insights_internet_ingestion_enabled
  internet_query_enabled     = var.appinsights_configuration.insights_internet_query_enabled
}
