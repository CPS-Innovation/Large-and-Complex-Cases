resource "azurerm_linux_web_app" "complex_cases_ui" {
  name                          = "${local.product_name_prefix}-ui"
  location                      = var.location
  resource_group_name           = azurerm_resource_group.rg_complex_cases.name
  service_plan_id               = azurerm_service_plan.asp_complex_cases_ui.id
  https_only                    = true
  virtual_network_subnet_id     = data.azurerm_subnet.complex_cases_placeholder_subnet.id
  public_network_access_enabled = false
  client_certificate_enabled    = false

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                  = data.azurerm_application_insights.complex_cases_ai.instrumentation_key
    "HostType"                                        = "Production"
    "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"        = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_complex_cases_ui_client_secret.id})"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpsccui.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.sacpsccui_ui_file_share.name
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

  sticky_settings {
    app_setting_names = ["HostType"]
  }

  site_config {
    ftps_state    = "FtpsOnly"
    http2_enabled = true
    #app_command_line       = "node polaris-ui/subsititute-config.js; npx serve -s"
    always_on = true
    #vnet_route_all_enabled = true
    use_32_bit_worker = false

    application_stack {
      node_version = "18-lts"
    }
  }

  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    default_provider       = "AzureActiveDirectory"
    unauthenticated_action = "AllowAnonymous"
    excluded_paths         = ["/status", "/complex-case-ui/build-version.txt"]

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
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings["APPINSIGHTS_INSTRUMENTATIONKEY"],
      app_settings["HostType"],
      app_settings["WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG"],
      app_settings["WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"],
      app_settings["WEBSITE_CONTENTOVERVNET"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["WEBSITE_DNS_ALT_SERVER"],
      app_settings["WEBSITE_DNS_SERVER"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"],
      app_settings["WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"],
      app_settings["WEBSITE_SWAP_WARMUP_PING_PATH"],
      app_settings["WEBSITE_SWAP_WARMUP_PING_STATUSES"],
      app_settings["WEBSITE_WARMUP_PATH"],
      app_settings["WEBSITES_ENABLE_APP_CACHE"]
    ]
  }
}

resource "azurerm_private_endpoint" "complex_cases_ui_pe" {
  name                = "${azurerm_linux_web_app.complex_cases_ui.name}-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_placeholder_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_web_app.complex_cases_ui.name}-psc"
    private_connection_resource_id = azurerm_linux_web_app.complex_cases_ui.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

resource "azuread_application" "complex_cases_ui" {
  display_name            = "${local.product_name_prefix}-ui"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/${local.product_name_prefix}-ui"]
  prevent_duplicate_names = true
  owners = [data.azuread_service_principal.terraform_service_principal.object_id]
  group_membership_claims = ["ApplicationGroup"]
  optional_claims {
    access_token {
      name = "groups"
    }
    id_token {
      name = "groups"
    }
    saml2_token {
      name = "groups"
    }
  }

  required_resource_access {
    resource_app_id = "00000002-0000-0000-c000-000000000000"

    resource_access {
      id   = "311a71cc-e848-46a1-bdf8-97ff7156d8e6"
      type = "Scope"
    }
  }

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000"

    resource_access {
      id   = "5f8c59db-677d-491f-a6b8-5f174b11ec1d"
      type = "Scope"
    }
  }

  #add api/function app resource access details below, when known/defined
  #required_resource_access {
    #resource_app_id = azuread_application.fa_redaction_log_reporting.client_id

    #dynamic "resource_access" {
    #  for_each = azuread_application.fa_redaction_log_reporting.api.0.oauth2_permission_scope
    #  iterator = scope

    #  content {
    #    id   = scope.value.id
    #    type = "Scope"
    #  }
    #}
  #}

  single_page_application {
    redirect_uris = var.environment.alias != "prod" ? ["http://localhost:3000"] : ["https://${local.product_name_prefix}-ui.azurewebsites.net"]
  }

  api {
    mapped_claims_enabled = true
    requested_access_token_version = 1
  }

  web {
    homepage_url = "https://${local.product_name_prefix}-ui.azurewebsites.net"
    redirect_uris = ["https://${local.product_name_prefix}-ui.azurewebsites.net/.auth/login/aad/callback",
    "https://getpostman.com/oauth2/callback"]

    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }

  depends_on = [
    #azuread_application.fa_redaction_log_reporting
  ]
  tags = local.common_tags
}

resource "azuread_application_password" "asap_complex_cases_ui" {
  application_id    = azuread_application.complex_cases_ui.id
  end_date_relative = "17520h"
}

resource "time_rotating" "schedule" {
  rotation_days = 90
}

resource "azuread_application_password" "e2e_test_secret" {
  application_id = azuread_application.complex_cases_ui.application_id
  display_name          = "e2e-tests client secret"
  rotate_when_changed = {
    rotation = time_rotating.schedule.id
  }
}

#store SPA ClientId and ClientSecret in terraform key vault so that they can be pulled in securely by the e2e tests, per environment
resource "azurerm_key_vault_secret" "kvs_ui_client_id" {
  #checkov:skip=CKV_AZURE_41:Ensure that the expiration date is set on all secrets
  #checkov:skip=CKV_AZURE_114:Ensure that key vault secrets have "content_type" set
  name         = "complex-cases-ui-client-id"
  value        = azuread_application.complex_cases_ui.client_id
  key_vault_id = data.azurerm_key_vault.terraform_key_vault.id
  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp
  ]
}

resource "azurerm_key_vault_secret" "kvs_ui_client_secret" {
  #checkov:skip=CKV_AZURE_41:Ensure that the expiration date is set on all secrets
  #checkov:skip=CKV_AZURE_114:Ensure that key vault secrets have "content_type" set
  name         = "complex-cases-ui-client-secret"
  value        = azuread_application_password.e2e_test_secret.value
  key_vault_id = data.azurerm_key_vault.terraform_key_vault.id
  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp
  ]
}