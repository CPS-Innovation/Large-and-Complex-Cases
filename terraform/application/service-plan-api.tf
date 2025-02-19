resource "azurerm_service_plan" "asp_complex_cases_api" {
  name                = "${local.product_name_prefix}-api-asp"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  os_type             = "Linux"
  sku_name            = var.service_plans.ui_service_plan_sku

  tags = local.common_tags
}

resource "azurerm_monitor_autoscale_setting" "amas_complex_cases_api" {
  name                = "${local.product_name_prefix}-api-amas"
  tags                = local.common_tags
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  target_resource_id  = azurerm_service_plan.asp_complex_cases_api.id
  profile {
    name = "Complex Cases API Performance Scaling Profile"
    capacity {
      default = 1
      minimum = 1
      maximum = 10
    }
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.asp_complex_cases_api.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 80
      }
      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT1M"
      }
    }
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.asp_complex_cases_api.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "LessThan"
        threshold          = 50
      }
      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT1M"
      }
    }
  }
}