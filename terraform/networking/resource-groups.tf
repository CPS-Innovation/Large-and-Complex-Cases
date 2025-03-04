resource "azurerm_resource_group" "rg_complex_cases_analytics" {
  name     = "rg-${local.product_name}-analytics"
  location = var.location
  tags     = local.common_tags
}