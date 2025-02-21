data "azurerm_resource_group" "networking_resource_group" {
  name = "rg-${local.product_name}-networking"
}