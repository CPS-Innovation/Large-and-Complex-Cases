resource "azurerm_linux_function_app_slot" "complex_cases_api_staging" {
  name                          = "staging"
  function_app_id               = azurerm_linux_function_app.complex_cases_api.id
  storage_account_name          = azurerm_storage_account.sacpsccapi.name
  storage_account_access_key    = azurerm_storage_account.sacpsccapi.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.complex_cases_api_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sacpsccapi.primary_connection_string
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet-isolated"
    "HostType"                                        = "Production"
    "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"        = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_complex_cases_api_client_secret.id})"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpsccapi.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.sacpsccapi_staging_file_share.name
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

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    application_insights_connection_string = data.azurerm_application_insights.complex_cases_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.complex_cases_ai.instrumentation_key
    always_on                              = true
    cors {
      allowed_origins = [
        "https://${local.product_prefix}-ui.azurewebsites.net",
        var.environment.alias == "dev" ? "http://localhost:3000" : ""
      ]
      support_credentials = true
    }
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "5"
    use_32_bit_worker                 = false
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
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }
}

resource "azurerm_private_endpoint" "complex_cases_api_staging_pe" {
  name                = "${azurerm_linux_function_app.complex_cases_api.name}-staging-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_endpoints_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.complex_cases_api.name}-staging-psc"
    private_connection_resource_id = azurerm_linux_function_app.complex_cases_api.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging"]
  }

  depends_on = [
    azurerm_linux_function_app_slot.complex_cases_api_staging
  ]
}