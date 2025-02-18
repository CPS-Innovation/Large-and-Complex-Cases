resource "azurerm_service_plan" "asp_complex_cases_egress" {
  name                         = "asp-${local.product_name_prefix}-egress"
  location                     = azurerm_resource_group.rg_complex_cases.location
  resource_group_name          = azurerm_resource_group.rg_complex_cases.name
  os_type                      = "Linux"
  sku_name                     = var.service_plans.egress_service_plan_sku
  tags                         = local.common_tags
  maximum_elastic_worker_count = var.service_plans.egress_plan_maximum_burst
}