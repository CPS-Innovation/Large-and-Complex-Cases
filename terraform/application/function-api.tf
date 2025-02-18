resource "azurerm_linux_function_app" "fa_api" {
  name                          = "${local.product_name_prefix}-api"
  location                      = azurerm_resource_group.rg_complex_cases.location
  resource_group_name           = azurerm_resource_group.rg_complex_cases.name
  service_plan_id               = azurerm_service_plan.asp_complex_cases_api.id
  storage_account_name          = azurerm_storage_account.sacpsccapi.name
  storage_account_access_key    = azurerm_storage_account.sacpsccapi.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.complex_cases_placeholder_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  builtin_logging_enabled       = false

  app_settings = {
    "AzureFunctionsJobHost__extensions__durableTask__storageProvider__MaxQueuePollingInterval"     = var.api_config.max_queue_polling_interval
    "AzureFunctionsJobHost__extensions__durableTask__storageProvider__ControlQueueBufferThreshold" = var.api_config.control_queue_buffer_threshold
    "AzureFunctionsJobHost__extensions__durableTask__MaxConcurrentActivityFunctions"               = var.api_config.max_concurrent_activity_functions
    "AzureFunctionsJobHost__extensions__durableTask__MaxConcurrentOrchestratorFunctions"           = var.api_config.max_concurrent_orchestrator_functions
    "AzureWebJobsStorage"                                                                          = azurerm_storage_account.sacpsccapi.primary_connection_string
    "Storage"                                         = azurerm_storage_account.sacpsccapi.primary_connection_string
    "ApiTaskHub"                              = "${local.product_name_prefix}api"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet-isolated"
    "HostType"                                        = "Production"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpsccapi.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.sacpsccapi.name
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
  }

  sticky_settings {
    app_setting_names = ["ApiTaskHub", "HostType"]
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    application_insights_connection_string = data.azurerm_application_insights.complex_cases_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.complex_cases_ai.instrumentation_key
    always_on                              = true
    cors {
      allowed_origins = [
        "https://${local.product_name_prefix}-ui.azurewebsites.net",
        "https://${local.product_name_prefix}-cmsproxy.azurewebsites.net",
        var.environment.alias == "dev" ? "http://localhost:3000" : ""
      ]
      support_credentials = true
    }
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    default_provider       = "AzureActiveDirectory"
    unauthenticated_action = "RedirectToLoginPage"
    excluded_paths         = ["/api/status"]

    # our default_provider:
    active_directory_v2 {
      tenant_auth_endpoint = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      #checkov:skip=CKV_SECRET_6:Base64 High Entropy String - Misunderstanding of setting "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = azuread_application.complex_cases_api.client_id
      allowed_audiences          = ["https://CPSGOVUK.onmicrosoft.com/${local.product_name_prefix}-api"]
    }

    login {
      token_store_enabled = false
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["AzureWebJobsStorage"],
      app_settings["ApiTaskHub"],
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
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"]
    ]
  }

  depends_on = [ azurerm_storage_account.sacpsccapi, azapi_resource.sacpsccapi_ui_file_share ]
}