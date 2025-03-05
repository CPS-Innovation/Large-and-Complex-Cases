resource "azurerm_service_plan" "asp_complex_cases_egressMock" {
  name                = "${local.product_prefix}-egressMock-asp"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  os_type             = "Linux"
  sku_name            = var.service_plans.egressMock_service_plan_sku
  zone_balancing_enabled = true
  worker_count = var.service_plans.egressMock_worker_count

  tags = local.common_tags
}

resource "azurerm_monitor_autoscale_setting" "amas_complex_cases_egressMock" {
  name                = "${local.product_prefix}-egressMock-amas"
  tags                = local.common_tags
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  target_resource_id  = azurerm_service_plan.asp_complex_cases_egressMock.id
  profile {
    name = "Complex Cases Egress Mock Performance Scaling Profile"
    capacity {
      default = var.service_capacity.egressMock_default_capacity
      minimum = var.service_capacity.egressMock_minimum_capacity
      maximum = var.service_capacity.egressMock_max_capacity
    }
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.asp_complex_cases_egressMock.id
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
        metric_resource_id = azurerm_service_plan.asp_complex_cases_egressMock.id
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

resource "azurerm_service_plan" "asp_complex_cases_netAppMock" {
  name                = "${local.product_prefix}-netAppMock-asp"
  location            = var.location
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  os_type             = "Linux"
  sku_name            = var.service_plans.netAppMock_service_plan_sku
  zone_balancing_enabled = true
  worker_count = var.service_plans.ui_worker_count

  tags = local.common_tags
}

resource "azurerm_monitor_autoscale_setting" "amas_complex_cases_netAppMock" {
  name                = "${local.product_prefix}-netAppMock-amas"
  tags                = local.common_tags
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  target_resource_id  = azurerm_service_plan.asp_complex_cases_netAppMock.id
  profile {
    name = "Complex Cases NetApp Mock Performance Scaling Profile"
    capacity {
      default = var.service_capacity.netAppMock_default_capacity
      minimum = var.service_capacity.netAppMock_minimum_capacity
      maximum = var.service_capacity.netAppMock_max_capacity
    }
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.asp_complex_cases_netAppMock.id
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
        metric_resource_id = azurerm_service_plan.asp_complex_cases_netAppMock.id
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