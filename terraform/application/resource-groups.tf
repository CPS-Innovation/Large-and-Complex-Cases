resource "azurerm_resource_group" "rg_complex_cases" {
  name     = "rg-${local.product_prefix}"
  location = var.location
}