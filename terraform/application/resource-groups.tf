resource "azurerm_resource_group" "rg_complex_cases" {
  name     = "rg-${local.group_product_name}-application"
  location = var.location
}