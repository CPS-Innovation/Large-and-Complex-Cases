resource "azurerm_linux_web_app_slot" "complex_cases_ui_staging" {
  name           = "staging"
  app_service_id = azurerm_linux_web_app.complex_cases_ui.id
  https_only     = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                  = data.azurerm_application_insights.complex_cases_ai.instrumentation_key
    "HostType"                                        = "Staging"
    "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"        = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_complex_cases_ui_client_secret.id})"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpsccui.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.sacpsccui_staging_file_share.name
    "WEBSITE_DNS_ALT_SERVER"                          = var.dns_alt_server
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "1"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"             = "10"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "" #fill in
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITE_WARMUP_PATH"                             = "" #fill in
    "WEBSITES_ENABLE_APP_CACHE"                       = "true"
  }

  site_config {
    ftps_state             = "FtpsOnly"
    http2_enabled          = true
    app_command_line       = "node complex-cases-ui/subsititute-config.js; npx serve -s"
    always_on              = true
    vnet_route_all_enabled = true
    use_32_bit_worker      = false

    application_stack {
      node_version = "18-lts"
    }
  }

  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    default_provider       = "AzureActiveDirectory"
    unauthenticated_action = "AllowAnonymous"
    excluded_paths         = ["/status", "/build-version.txt"]

    active_directory_v2 {
      tenant_auth_endpoint = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      #checkov:skip=CKV_SECRET_6:Base64 High Entropy String - Misunderstanding of setting "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = azuread_application.complex_cases_ui.client_id
    }

    login {
      token_store_enabled = true
    }
  }

  logs {
    detailed_error_messages = true
    failed_request_tracing  = true

    http_logs {
      file_system {
        retention_in_days = 7
        retention_in_mb = 25
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }
}

resource "azurerm_private_endpoint" "complex_cases_ui_staging_pe" {
  name                = "${azurerm_linux_web_app.complex_cases_ui.name}-staging-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_endpoints_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_web_app.complex_cases_ui.name}-staging-psc"
    private_connection_resource_id = azurerm_linux_web_app.complex_cases_ui.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging"]
  }

  depends_on = [
    azurerm_linux_web_app_slot.complex_cases_ui_staging
  ]
}