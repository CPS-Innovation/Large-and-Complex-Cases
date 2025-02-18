resource "azurerm_linux_function_app" "complex_cases_netapp" {
  name                          = "${local.product_name_prefix}-netapp"
  location                      = azurerm_resource_group.rg_complex_cases.location
  resource_group_name           = azurerm_resource_group.rg_complex_cases.name
  service_plan_id               = azurerm_service_plan.asp_complex_cases_netapp.id
  storage_account_name          = azurerm_storage_account.sacpsccnetapp.name
  storage_account_access_key    = azurerm_storage_account.sacpsccnetapp.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.complex_cases_placeholder_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sacpsccnetapp.primary_connection_string
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet-isolated"
    "HostType"                                        = "Production"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.scale_controller_logging.netapp
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpsccnetapp.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.sacpsccnetapp_file_share.name
    "WEBSITE_DNS_ALT_SERVER"                          = var.dns_alt_server
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "1"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"             = "10"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITE_WARMUP_PATH"                             = "/api/status"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"          = "1"
  }

  sticky_settings {
    app_setting_names = ["HostType"]
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    runtime_scale_monitoring_enabled       = true
    vnet_route_all_enabled                 = true
    elastic_instance_minimum               = var.service_plans.netapp_always_ready_instances
    app_scale_limit                        = var.service_plans.netapp_maximum_scale_out_limit
    pre_warmed_instance_count              = var.service_plans.netapp_always_ready_instances
    application_insights_connection_string = data.azurerm_application_insights.complex_cases_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.complex_cases_ai.instrumentation_key
    health_check_path                      = "/api/status"
    health_check_eviction_time_in_min      = "2"
    use_32_bit_worker                      = false
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = false
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "AllowAnonymous"
  }

  lifecycle {
    ignore_changes = [
      app_settings["AzureWebJobsStorage"],
      app_settings["HostType"],
      app_settings["SCALE_CONTROLLER_LOGGING_ENABLED"],
      app_settings["WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG"],
      app_settings["WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"],
      app_settings["WEBSITE_CONTENTOVERVNET"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["WEBSITE_DNS_ALT_SERVER"],
      app_settings["WEBSITE_DNS_SERVER"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"],
      app_settings["WEBSITE_RUN_FROM_PACKAGE"],
      app_settings["WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"],
      app_settings["WEBSITE_SWAP_WARMUP_PING_PATH"],
      app_settings["WEBSITE_SWAP_WARMUP_PING_STATUSES"],
      app_settings["WEBSITE_WARMUP_PATH"],
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"]
    ]
  }
}

resource "azuread_application" "complex_cases_netapp" {
  display_name            = "${local.product_name_prefix}-netapp"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/${local.product_name_prefix}-netapp"]
  prevent_duplicate_names = true

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000"

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
      type = "Scope"
    }
  }
}

resource "azurerm_private_endpoint" "complex_cases_netapp_pe" {
  name                = "${azurerm_linux_function_app.complex_cases_netapp.name}-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_placeholder_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.complex_cases_netapp.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.complex_cases_netapp.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}