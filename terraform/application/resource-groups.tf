resource "azurerm_resource_group" "rg_complex_cases" {
  name     = "rg-${local.product_name}${local.resource_suffix}"
  location = var.location
}