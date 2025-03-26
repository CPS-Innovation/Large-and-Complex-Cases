resource "azurerm_resource_group" "rg_complex_cases" {
  name     = "rg-${local.group_product_name}${local.resource_suffix}-application"
  location = var.location
}