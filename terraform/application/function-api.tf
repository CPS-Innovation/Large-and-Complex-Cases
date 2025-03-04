resource "azurerm_linux_function_app" "complex_cases_api" {
  name                          = "${local.product_prefix}-api"
  location                      = azurerm_resource_group.rg_complex_cases.location
  resource_group_name           = azurerm_resource_group.rg_complex_cases.name
  service_plan_id               = azurerm_service_plan.asp_complex_cases_api.id
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
    "Storage"                                         = azurerm_storage_account.sacpsccapi.primary_connection_string
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet-isolated"
    "HostType"                                        = "Production"
    "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"        = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_complex_cases_api_client_secret.id})"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpsccapi.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.sacpsccapi_file_share.name
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
    app_setting_names = ["HostType"]
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
    health_check_eviction_time_in_min = "2"
    use_32_bit_worker                 = false
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
      allowed_audiences          = ["https://CPSGOVUK.onmicrosoft.com/${local.product_prefix}-api"]
    }

    login {
      token_store_enabled = false
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["AzureWebJobsStorage"],
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

  depends_on = [azurerm_storage_account.sacpsccapi, azapi_resource.sacpsccapi_file_share]
}

resource "azuread_application" "complex_cases_api" {
  display_name            = "${local.product_prefix}-api"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/${local.product_prefix}-api"]
  prevent_duplicate_names = true

  api {
    requested_access_token_version = 1
    oauth2_permission_scope {
      admin_consent_description  = "Access Complex Cases API as a user"
      admin_consent_display_name = "Access Complex Cases API as a user"
      id                         = element(random_uuid.random_id[*].result, 1)
      enabled                    = true
      type                       = "Admin"
      user_consent_description   = "Access Complex Cases API as a user"
      user_consent_display_name  = "Access Complex Cases API as a user"
      value                      = "user_impersonation"
    }
  }

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["Files.ReadWrite.All"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.app_role_ids["Files.ReadWrite.All"]
      type = "Role"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["FileStorageContainer.Manage"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["FileStorageContainer.Selected"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.app_role_ids["FileStorageContainer.Selected"]
      type = "Role"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.app_role_ids["Mail.ReadWrite"]
      type = "Role"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["Mail.ReadWrite.Shared"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.app_role_ids["Mail.Send"]
      type = "Role"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["Sites.Read.All"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read.All"]
      type = "Scope"
    }
  }

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["Office365SharePointOnline"]

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["Container.Manager"]
      type = "Scope"
    }
  }

  required_resource_access {
    resource_app_id = azuread_application.sharepoint_embedded.client_id

    dynamic "resource_access" {
      for_each = azuread_application.complex_cases_api.api.0.oauth2_permission_scope
      iterator = scope

      content {
        id   = scope.value.id
        type = "Scope"
      }
    }
  }

  web {
    redirect_uris = ["https://${local.product_prefix}-ui.azurewebsites.net/.auth/login/aad/callback"]

    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = true
    }
  }
}

resource "azurerm_private_endpoint" "complex_cases_api_pe" {
  name                = "${azurerm_linux_function_app.complex_cases_api.name}-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_api_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.complex_cases_api.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.complex_cases_api.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

resource "azuread_application_password" "pwd_complex_cases_api" {
  application_id = azuread_application.complex_cases_api.id
  rotate_when_changed = {
    rotation = time_rotating.schedule_api.id
  }
}

resource "time_rotating" "schedule_api" {
  rotation_days = 90
}

resource "azurerm_key_vault_secret" "kvs_complex_cases_api_client_secret" {
  name         = "api-client-secret${local.resource_suffix}"
  value        = azuread_application_password.pwd_complex_cases_api.value
  key_vault_id = azurerm_key_vault.kv_complex_cases.id
  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azuread_application_password.pwd_complex_cases_api
  ]
}